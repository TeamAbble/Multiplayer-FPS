using FMODUnity;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
public class AutomaticWeapon: BaseWeapon
{

    [System.Serializable] public class Tracer
    {
        public float lerp;
        public Vector3 start, end;
        public float speed;
        public GameObject tracer;
    }
    List<Tracer> tracers = new();






    [SerializeField] protected bool canFire;
    [SerializeField] protected float roundsPerMinute;
    [SerializeField] protected float timeBetweenShots;
    [SerializeField] protected bool firePressed;
    public enum FireMode
    {
        single = 0,
        burst = 1,
        auto = 2,
    }
    [SerializeField] protected List<FireMode> fireModes;
    [SerializeField] protected int fireModeIndex;
    public string FireModeDisplayName => System.Enum.GetName(typeof(FireMode), fireModes[fireModeIndex]);
    [SerializeField] protected float fireWindupTime;
    protected float currentWindup;
    [SerializeField] StudioEventEmitter gunshotEmitter;
    public FireMode CurrentFireMode => fireModes[fireModeIndex];
    protected bool WindupReady => fireWindupTime == 0 || currentWindup >= fireWindupTime;
    [SerializeField] protected float maxDamage, minDamage, maxRange, bulletRadius;
    [SerializeField] protected float falloffStartRange, falloffEndRange;
    [SerializeField] protected AnimationCurve damageFalloffCurve;
    [SerializeField] protected GameObject bulletTracer;
    [SerializeField] protected float tracerSpeed;
    [SerializeField] protected GameObject projectile;
    [SerializeField] protected float projectileForce;

    [SerializeField] protected int burstFireRounds;
    [SerializeField] protected int currentBurstFireRounds;
    [SerializeField] protected float burstInterval;
    [SerializeField] protected bool burstFiring;
    [SerializeField] protected Transform tracerOrigin;
    public float Damage(RaycastHit hit)
    {
        float dmg = 0;



        return dmg;
    }
    void CreateTracer(Vector3 start, Vector3 end)
    {
        GameObject t = Instantiate(bulletTracer, tracerOrigin.position, Quaternion.identity);
        tracers.Add(new()
        {
            start = start,
            end = end,
            speed = tracerSpeed / Vector3.Distance(start, end),
            tracer = t
        });
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        canFire = true;
    }

    private void FixedUpdate()
    {
        currentWindup = Mathf.Clamp(currentWindup + (fireInput ? Time.fixedDeltaTime : -Time.fixedDeltaTime), 0, fireWindupTime);
        if(tracers.Count > 0)
        {
            tracers.ForEach(t =>
            {
                t.lerp += Time.fixedDeltaTime * t.speed;
                t.tracer.transform.position = Vector3.Lerp(t.start, t.end, t.lerp);
            });
            tracers.RemoveAll(x => x.lerp >= 1);
        }

        if (fireInput)
        {
            if (canFire && WindupReady && HasAmmo)
            {
                switch (CurrentFireMode)
                {
                    case FireMode.single:
                        if (!firePressed)
                        {
                            Attack();
                            firePressed = true;
                        }
                        break;
                    case FireMode.burst:
                        if (!burstFiring)
                        {
                            StartCoroutine(BurstFire());
                        }
                        break;
                    case FireMode.auto:
                        Attack();
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            firePressed = false;
        }
    }
    public bool RaycastAttack(out RaycastHit hit, out Vector3 endPoint)
    {
        Vector3 direction = manager.fireOrigin.forward;

        if (Physics.Raycast(manager.fireOrigin.position, direction, out hit, maxRange, GameManager.Instance.attackLayer, QueryTriggerInteraction.Ignore))
            endPoint = hit.point;
        else endPoint = manager.fireOrigin.position + direction * maxRange;





        return hit.collider;
    }
    IEnumerator BurstFire()
    {
        burstFiring = true;
        while (currentBurstFireRounds < burstFireRounds && currentAmmo > 0)
        {
            Attack();
            yield return new WaitForSeconds(burstInterval);
        }
        burstFiring = false;
        currentBurstFireRounds = 0;
        Invoke(nameof(ResetFire), timeBetweenShots);
    }

    [ServerRpc(RunLocally = true)]
    protected override void ServerAttack()
    {
        canFire = false;

        print("Fired automatic weapon on server");
        if(!burstFiring)
            Invoke(nameof(ResetFire), timeBetweenShots);
        if (projectile)
        {

        }
        else
        {
            if (RaycastAttack(out RaycastHit hit, out Vector3 endPoint))
            {
                if (IsServerInitialized)
                {
                    if (hit.rigidbody)
                    {
                        hit.rigidbody.AddForceAtPosition(-hit.normal * Damage(hit), hit.point);
                        if (hit.rigidbody.transform.parent.TryGetComponent(out PlayerManager pm))
                        {
                            //We've hit a player! yippee!
                            
                        }
                    }
                }
            }
        }

    }
    [ObserversRpc()]
    protected override void ClientAttack()
    {
        print("Fired automatic weapon on this client");
        if(projectile)
        {

        }
        else
        {
            if (RaycastAttack(out RaycastHit hit, out Vector3 endPoint))
            {

            }
            CreateTracer(tracerOrigin.position, endPoint);
            if (!gunshotEmitter.EventReference.IsNull)
                gunshotEmitter.Play();
        }

    }
    public override void Attack()
    {
        if (!IsOwner)
            return;

        ServerAttack();
        ClientAttack();
    }
    protected override void HitFeedback(RaycastHit hit)
    {
        //We've hit something physical
        GameObject he;
        if (hit.rigidbody && hit.rigidbody.TryGetComponent(out PhysicalPlayer pp))
        {
            he = Instantiate(playerHitEffect, hit.point, Quaternion.identity, pp.transform);
            //We've hit a player, so we need to use the player hit effect
            he.transform.up = hit.normal;
        }
        else
        {
            he = Instantiate(objectHitEffect, hit.point, Quaternion.identity, hit.collider.transform);
        }
        he.transform.up = hit.normal;
        Destroy(he, 10);
        if (hit.collider)
        {
            HitFeedback(hit);
        }
    }
    void ResetFire()
    {
        canFire = true;
    }
    protected override void OnValidate()
    {
        base.OnValidate();
        roundsPerMinute = Mathf.Abs(roundsPerMinute);
        if(roundsPerMinute != 0)
        {
            timeBetweenShots = 1 / (roundsPerMinute / 60);
        }
    }
}
