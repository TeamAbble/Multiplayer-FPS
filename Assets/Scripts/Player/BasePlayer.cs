using FishNet.Object;
using UnityEngine;

public class BasePlayer : NetworkBehaviour
{
    [SerializeField] internal Vector2 moveInput, lookInput, moveSpeed, lookSpeed;
    [SerializeField] internal Vector2 lookPitchClamp;
    [SerializeField] internal float moveDampTime;
    internal Vector3 moveDampVelocity;
    internal Vector3 moveVec;
    [SerializeField] internal Rigidbody rb;
    [SerializeField] internal Renderer[] teamColourRenderers;
    [SerializeField] internal PlayerManager playerManager;

    [SerializeField] internal float lookPitch, lookPitchOffset, lookYaw;
    private void OnLevelWasLoaded(int level)
    {
        if (GameModeController.instance)
        {
            //Set the team colour
            if (teamColourRenderers.Length > 0)
            {
                foreach (var item in teamColourRenderers)
                {
                    for (int i = 0; i < item.materials.Length; i++)
                    {
                        item.materials[i].SetColor("_BaseColor", GameModeController.instance.teamColours[playerManager.teamNumber.Value]);
                    }
                }
            }
        }
    }
    private void FixedUpdate()
    {
        if (IsOwner)
        {
            Move();
        }
    }
    private void Update()
    {
        if (IsOwner)
        {
            Look();
        }
    }
    protected virtual Vector3 TargetMoveVector() => transform.rotation * new Vector3(moveInput.x * moveSpeed.x, 0f, moveInput.y * moveSpeed.y);
    protected virtual void Move()
    {
        moveVec = Vector3.SmoothDamp(moveVec, TargetMoveVector(), ref moveDampVelocity, moveDampTime);
        rb.Move(rb.position + moveVec * Time.fixedDeltaTime, transform.rotation);
    }
    protected virtual void Look()
    {
        
    }
}
