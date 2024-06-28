using FishNet.Object;
using Unity.Cinemachine;
using UnityEngine;

public class PhysicalPlayer : BasePlayer
{

    [SerializeField] internal Transform head;
    [SerializeField] protected float groundCheckRadius, groundCheckDistance;
    [SerializeField] protected Vector3 groundCheckNormal;
    [SerializeField] protected LayerMask groundMask;
    [SerializeField] bool grounded;
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
            //We've just left the ground
            if (grounded)
            {
                rb.AddForce(transform.rotation * new Vector3(moveInput.x * moveSpeed.x, 0, moveInput.y * moveSpeed.y), ForceMode.Impulse);
            }
        }
        grounded = hit.collider;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
        Gizmos.DrawWireSphere(transform.position - transform.up * groundCheckDistance, groundCheckRadius);
    }
}
