using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class BaseWeapon : NetworkBehaviour
{
    protected WeaponManager manager;
    public bool fireInput;
    public bool secondaryInput;
    [SerializeField] protected bool useAmmo;
    [SerializeField] protected int currentAmmo;
    [SerializeField] protected int maxAmmo;
    [SerializeField] protected bool hasADS;
    [SerializeField] protected Vector2 adsSpeed;
    protected bool HasAmmo => !useAmmo || currentAmmo > 0;

    public GameObject objectHitEffect, playerHitEffect;

    public override void OnStartClient()
    {
        base.OnStartClient();
        manager = GetComponentInParent<WeaponManager>();
        currentAmmo = maxAmmo;
    }
    [ObserversRpc]
    public void ReloadWeapon()
    {
        currentAmmo = maxAmmo;
    }
    public virtual void Attack()
    {

        if (!IsOwner)
            return;

        ServerAttack();
        ClientAttack();
        
    }
    [ServerRpc(RunLocally = false)]
    protected virtual void ServerAttack()
    {
        print($"fired {name} on server");
    }
    [ObserversRpc()]
    protected virtual void ClientAttack()
    {
        print($"fired {name} on this client");
    }
    [ObserversRpc()]
    protected virtual void HitFeedback(RaycastHit hit)
    {

    }
}
