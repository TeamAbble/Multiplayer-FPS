using FishNet.Object;
using Unity.Cinemachine;
using UnityEngine;

public class PhysicalPlayer : BasePlayer
{

    [SerializeField] internal Transform head;
    [SerializeField] protected float groundCheckRadius, groundCheckDistance;
    [SerializeField] protected Vector3 groundCheckNormal;
    [SerializeField] protected LayerMask groundMask;

    protected override void Look()
    {
        base.Look();
        transform.localRotation = Quaternion.Euler(0, lookYaw, 0);
        head.localRotation = Quaternion.Euler(lookPitch, 0, 0);
    }

    protected override Vector3 TargetMoveVector()
    {
        Vector3 vec = Vector3.ProjectOnPlane(base.TargetMoveVector(), groundCheckNormal);
        return vec;
    }

    protected override void Move()
    {
        if (Physics.SphereCast(transform.position, groundCheckRadius, -transform.up, out RaycastHit hit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            base.Move();
            groundCheckNormal = hit.normal;
        }
        else
        {

            return;
        }

    }
}
