using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour, IDirectionalInteractor
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;

    public Vector2 FacingDirection { get; private set; } = Vector2.left;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        movement = new Vector2(x, y).normalized;

        // Remember the last cardinal direction.
        if (x != 0)
        {
            FacingDirection = new Vector2(Mathf.Sign(x), 0f);

            if (spriteRenderer != null)
                spriteRenderer.flipX = x > 0;
        }
        else if (y != 0)
        {
            FacingDirection = new Vector2(0f, Mathf.Sign(y));
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(
            rb.position + movement * moveSpeed * Time.fixedDeltaTime
        );
    }
}