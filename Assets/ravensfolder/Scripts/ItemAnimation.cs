using UnityEngine;

public class ItemAnimation : MonoBehaviour
{
    [Header("Scale Animation")]
    [SerializeField] private float scaleAmount = 0.1f;
    [SerializeField] private float speed = 4f;

    [Header("Highlight")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField, Range(0f, 1f)] private float highlightIntensity = 0.35f;
    [SerializeField] private float highlightSpeed = 2f;

    private Vector3 startScale;
    private SpriteRenderer spriteRenderer;
    private Color startColor;

    private void Start()
    {
        startScale = transform.localScale;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            startColor = spriteRenderer.color;
    }

    private void Update()
    {
        // Пульсация размера
        float scale = 1f + Mathf.Sin(Time.time * speed) * scaleAmount;
        transform.localScale = startScale * scale;

        // Пульсирующая подсветка
        if (spriteRenderer != null)
        {
            float pulse = (Mathf.Sin(Time.time * highlightSpeed) + 1f) / 2f;

            spriteRenderer.color = Color.Lerp(
                startColor,
                highlightColor,
                pulse * highlightIntensity
            );
        }
    }
}