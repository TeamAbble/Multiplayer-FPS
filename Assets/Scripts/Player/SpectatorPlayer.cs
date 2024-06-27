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
        lookPitch = Mathf.Clamp(lookPitch - (lookInput.y * lookSpeed.y) * Time.deltaTime, lookPitchClamp.x, lookPitchClamp.y) + lookPitchOffset;
        lookYaw += lookInput.x * lookSpeed.x * Time.deltaTime;
        lookYaw %= 360;
        transform.localRotation = Quaternion.Euler(lookPitch, lookYaw, 0);
    }
}
