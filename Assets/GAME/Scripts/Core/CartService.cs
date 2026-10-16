using System.Collections;
using UnityEngine;

public class CartService : MonoBehaviour
{
    [Header("Cart")]
    [SerializeField] private CartFollower cartPrefab;
    [SerializeField] private CartFollower sceneCart;
    [SerializeField, Min(0.1f)] private float followDistance = 0.8f;
    [SerializeField, Min(0.5f)] private float hitchRange = 1.8f;
    [SerializeField] private string cartCapacitySourceId = "cart";
    [SerializeField, Min(1)] private int cartCapacityBonus = 12;
    [SerializeField] private bool startHitched = false;

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

    private IEnumerator Start()
    {
        float waitTime = 0f;
        bool warningShown = false;

        // Wait until InventoryService registers itself.
        while (activeCart == null)
        {
            GameServices services = GameServices.Instance;

            if (services != null &&
                services.TryGet<InventoryService>(
                    out InventoryService inventory))
            {
                InitializeAlwaysAvailableCart(inventory);
                yield break;
            }

            waitTime += Time.unscaledDeltaTime;

            if (!warningShown && waitTime >= 3f)
            {
                Debug.LogWarning(
                    "CartService is waiting for InventoryService registration.",
                    this);

                warningShown = true;
            }

            yield return null;
        }
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
        if (GameplayInputGate.IsBlocked ||
            !Input.GetKeyDown(toggleKey))
        {
            return;
        }

        ToggleCart();
    }

    private void InitializeAlwaysAvailableCart(InventoryService inventory)
    {
        if (activeCart != null || inventory == null)
            return;

        owner = inventory.transform;

        CartFollower instance = sceneCart;

        if (instance == null)
            instance = FindObjectOfType<CartFollower>();

        bool spawnedByService = false;

        if (instance == null)
        {
            if (cartPrefab == null)
            {
                Debug.LogError(
                    "CartService: Assign a Scene Cart or Cart Prefab.",
                    this);
                return;
            }

            Vector3 spawnPosition =
                owner.position + Vector3.down * followDistance;

            instance = Instantiate(
                cartPrefab,
                spawnPosition,
                Quaternion.identity);

            spawnedByService = true;
        }

        if (string.IsNullOrWhiteSpace(cartCapacitySourceId) ||
            cartCapacityBonus <= 0)
        {
            Debug.LogError(
                "CartService: Invalid cart capacity settings.",
                this);

            if (spawnedByService)
                Destroy(instance.gameObject);

            return;
        }

        if (!inventory.SetCapacityBonus(
            cartCapacitySourceId,
            cartCapacityBonus,
            InventorySlotGroup.Cart))
        {
            Debug.LogError(
                "CartService: Could not add cart inventory slots.",
                this);

            if (spawnedByService)
                Destroy(instance.gameObject);

            return;
        }

        instance.BindOwner(
            owner,
            followDistance,
            startHitched,
            snapBehindOwner: false);

        activeCart = instance;

        GameEvents.Publish(
            new CartStateChangedEvent(true, activeCart.IsHitched));

        Debug.Log(
            startHitched
                ? "The cart is ready and hitched. Press G to park it."
                : "The cart is ready. Get close and press G to hitch it.",
            this);
    }

    // Retained for compatibility with ShopService.
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
            Debug.LogWarning(
                "CartService: Cart Prefab is not assigned.",
                this);
            return false;
        }

        owner = inventory.transform;

        Vector3 spawnPosition =
            owner.position + Vector3.down * followDistance;

        CartFollower instance = Instantiate(
            cartPrefab,
            spawnPosition,
            Quaternion.identity);

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
            Debug.LogWarning(
                "CartService: Cart has not initialized. Check Cart Prefab / Scene Cart.",
                this);
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

        float distance = Vector2.Distance(
            owner.position,
            activeCart.transform.position);

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