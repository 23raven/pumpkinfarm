using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //движение камеры наврятли придется менять
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;

    [SerializeField] private Vector2 offset = new Vector2(0f, 1f);

    private float cameraZ;

    private void Start()
    {
        cameraZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, cameraZ);

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
    }
}