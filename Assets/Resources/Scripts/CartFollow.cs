using UnityEngine;

public class CartFollow : MonoBehaviour
{
    public Transform target;
    [SerializeField] private float followSpeed = 4f;
    [SerializeField] private float stopDistance = 1.2f;

    private float initialZ;

    private void Start()
    {
        initialZ = transform.position.z;
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(target.position.x, target.position.y, initialZ);
        float distance = Vector2.Distance(transform.position, targetPos);

        if (distance > stopDistance)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.fixedDeltaTime);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}