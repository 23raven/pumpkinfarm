using UnityEngine;

public class CartFollower : MonoBehaviour
{
    [Header("Following")]
    [SerializeField, Min(0.1f)] private float followDistance = 0.8f;
    [SerializeField, Min(0f)] private float smoothTime = 0.12f;
    [SerializeField] private Vector3 positionOffset;

    private Transform ownerTarget;
    private Transform target;
    private Vector3 previousTargetPosition;
    private Vector3 velocity;
    private Vector2 lastMoveDirection = Vector2.up;

    public bool IsHitched { get; private set; }

    // Existing behavior retained for callers that explicitly initialize a newly spawned cart.
    public void Initialize(Transform followTarget, float distance = -1f)
    {
        BindOwner(followTarget, distance, startHitched: true, snapBehindOwner: true);
    }

    // Registers the owner without requiring a purchase. Scene carts can remain where they were placed.
    public void BindOwner(
        Transform followTarget,
        float distance = -1f,
        bool startHitched = false,
        bool snapBehindOwner = false)
    {
        ownerTarget = followTarget;

        if (distance > 0f)
            followDistance = distance;

        if (ownerTarget == null)
        {
            SetHitched(false);
            return;
        }

        if (snapBehindOwner)
        {
            Vector3 initialPosition = ownerTarget.position
                - (Vector3)(lastMoveDirection * followDistance)
                + positionOffset;
            initialPosition.z = transform.position.z;
            transform.position = initialPosition;
        }

        SetHitched(startHitched);
    }

    public void SetHitched(bool hitched)
    {
        if (hitched)
        {
            if (ownerTarget == null)
                return;

            target = ownerTarget;
            previousTargetPosition = target.position;
            velocity = Vector3.zero;
            IsHitched = true;
        }
        else
        {
            target = null;
            velocity = Vector3.zero;
            IsHitched = false;
        }
    }

    private void LateUpdate()
    {
        if (!IsHitched || target == null)
            return;

        Vector3 targetDelta = target.position - previousTargetPosition;
        if (targetDelta.sqrMagnitude > 0.0001f)
            lastMoveDirection = new Vector2(targetDelta.x, targetDelta.y).normalized;

        previousTargetPosition = target.position;

        Vector3 desiredPosition = target.position
            - (Vector3)(lastMoveDirection * followDistance)
            + positionOffset;
        desiredPosition.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );
    }
}
