using UnityEngine;

public class CartService : MonoBehaviour
{
    [Header("Cart")]
    [SerializeField] private CartFollower cartPrefab;
    [SerializeField, Min(0.1f)] private float followDistance = 0.8f;
    [SerializeField, Min(0.5f)] private float hitchRange = 1.8f;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.G;

    private CartFollower activeCart;
    private Transform owner;

    public bool HasCart => activeCart != null;
    public bool IsHitched => activeCart != null && activeCart.IsHitched;

    private void OnEnable()
    {
        GameServices services = GameServices.Instance;
        if (services == null)
        {
            Debug.LogError("GameServices is missing.", this);
            return;
        }

        services.Register(this);
    }

    private void OnDisable()
    {
        if (activeCart != null)
        {
            activeCart.SetHitched(false);
            GameEvents.Publish(new CartStateChangedEvent(true, false));
        }

        GameServices services = GameServices.Instance;
        if (services != null)
            services.Unregister(this);
    }

    private void Update()
    {
        if (GameplayInputGate.IsBlocked || !Input.GetKeyDown(toggleKey))
            return;

        ToggleCart();
    }

    public bool TryActivate(
        string productId,
        int capacityBonus,
        InventoryService inventory)
    {
        if (string.IsNullOrWhiteSpace(productId) ||
            capacityBonus <= 0 ||
            inventory == null ||
            activeCart != null)
        {
            return false;
        }

        if (cartPrefab == null)
        {
            Debug.LogWarning("CartService: Cart Prefab is not assigned.", this);
            return false;
        }

        owner = inventory.transform;
        Vector3 spawnPosition = owner.position + Vector3.down * followDistance;
        CartFollower instance = Instantiate(cartPrefab, spawnPosition, Quaternion.identity);
        if (instance == null)
            return false;

        if (!inventory.SetCapacityBonus(
            productId,
            capacityBonus,
            InventorySlotGroup.Cart))
        {
            Destroy(instance.gameObject);
            return false;
        }

        instance.Initialize(owner, followDistance);
        activeCart = instance;
        GameEvents.Publish(new CartStateChangedEvent(true, true));
        Debug.Log("Cart hitched. Press G to park or hitch it again.");
        return true;
    }

    public void ToggleCart()
    {
        if (activeCart == null)
        {
            Debug.Log("You don't own a cart yet.");
            return;
        }

        if (activeCart.IsHitched)
        {
            activeCart.SetHitched(false);
            GameEvents.Publish(new CartStateChangedEvent(true, false));
            Debug.Log("Cart parked. Items in cart slots are stored safely.");
            return;
        }

        if (owner == null)
        {
            Debug.LogWarning("CartService: Player target is missing.", this);
            return;
        }

        float distance = Vector2.Distance(owner.position, activeCart.transform.position);
        if (distance > hitchRange)
        {
            Debug.Log("Get closer to the parked cart to hitch it.");
            return;
        }

        activeCart.SetHitched(true);
        GameEvents.Publish(new CartStateChangedEvent(true, true));
        Debug.Log("Cart hitched. Movement speed reduced.");
    }
}
