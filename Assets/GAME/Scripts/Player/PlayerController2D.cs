using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour, IDirectionalInteractor
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField, Range(0.1f, 1f)] private float cartSpeedMultiplier = 0.75f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;
    private bool cartHitched;

    public Vector2 FacingDirection { get; private set; } = Vector2.left;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<CartStateChangedEvent>(OnCartStateChanged);

        if (GameEvents.TryGetLatest<CartStateChangedEvent>(out var state))
            cartHitched = state.HasCart && state.IsHitched;
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<CartStateChangedEvent>(OnCartStateChanged);
    }

    private void OnCartStateChanged(CartStateChangedEvent message)
    {
        cartHitched = message.HasCart && message.IsHitched;
    }

    private void Update()
    {
        if (GameplayInputGate.IsBlocked)
        {
            movement = Vector2.zero;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        movement = new Vector2(x, y).normalized;

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
        if (GameplayInputGate.IsBlocked)
            return;

        float speed = moveSpeed * (cartHitched ? cartSpeedMultiplier : 1f);
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}
