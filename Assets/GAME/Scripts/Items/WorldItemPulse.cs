using UnityEngine;

public class WorldItemPulse : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField, Range(0f, 0.3f)]
    private float scaleAmount = 0.12f;

    [SerializeField, Min(0.1f)]
    private float cyclesPerSecond = 0.8f;

    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    private void Update()
    {
        float wave =
            (Mathf.Sin(Time.time * cyclesPerSecond * Mathf.PI * 2f) + 1f)
            * 0.5f;

        float scaleMultiplier = 1f + wave * scaleAmount;

        transform.localScale = initialScale * scaleMultiplier;
    }
}   