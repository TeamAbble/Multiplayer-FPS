using UnityEngine;

public class SpectatorPlayer : BasePlayer
{
    [SerializeField] protected float verticalInput;
    [SerializeField] protected float verticalMoveSpeed;


    protected override Vector3 TargetMoveVector()
    {
        
        return transform.rotation * new Vector3(moveInput.x * moveSpeed.x, verticalInput * verticalMoveSpeed, moveInput.y * moveSpeed.y);
    }
    protected override void Look()
    {
        base.Look();

        transform.localRotation = Quaternion.Euler(lookPitch, lookYaw, 0);
    }
}
