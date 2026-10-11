using System.Collections.Generic;
using UnityEngine;

public class InventoryHUDController : MonoBehaviour
{
    [Header("Cards")]
    [SerializeField] private Transform cardRoot;
    [SerializeField] private InventorySlotCardView cardPrefab;

    private readonly List<InventorySlotCardView> cards =
        new List<InventorySlotCardView>();

    private Canvas rootCanvas;

    private void Awake()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        rootCanvas = canvas != null ? canvas.rootCanvas : null;

        if (cardRoot == null)
            Debug.LogWarning("InventoryHUDController: Card Root is not assigned.", this);

        if (cardPrefab == null)
            Debug.LogWarning("InventoryHUDController: Card Prefab is not assigned.", this);
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<InventoryLayoutChangedEvent>(OnInventoryChanged);
        RefreshLatest();
    }

    private void Start()
    {
        // The inventory service may register after this view is enabled.
        RefreshLatest();
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<InventoryLayoutChangedEvent>(OnInventoryChanged);
    }

    private void OnInventoryChanged(InventoryLayoutChangedEvent message)
    {
        if (message.Slots == null || cardRoot == null || cardPrefab == null)
            return;

        EnsureCardCount(message.Slots.Length);

        var groupIndices = new Dictionary<InventorySlotGroup, int>
    {
        { InventorySlotGroup.Base, 0 },
        { InventorySlotGroup.Backpack, 0 },
        { InventorySlotGroup.Cart, 0 }
    };

        for (int i = 0; i < message.Slots.Length; i++)
        {
            InventorySlotDisplayData slot = message.Slots[i];

            int groupIndex = groupIndices[slot.Group] + 1;
            groupIndices[slot.Group] = groupIndex;

            string groupLabel =
                $"{slot.Group.ToString().ToUpperInvariant()} {groupIndex}";

            // Hide all cart cards while the cart is parked.
            bool shouldShow =
                slot.Group != InventorySlotGroup.Cart || slot.IsAccessible;

            cards[i].gameObject.SetActive(shouldShow);

            cards[i].Bind(
                i,
                slot,
                i == message.SelectedSlotIndex,
                groupLabel,
                rootCanvas,
                this
            );
        }
    }

    private void EnsureCardCount(int requiredCount)
    {
        while (cards.Count < requiredCount)
        {
            InventorySlotCardView card = Instantiate(cardPrefab, cardRoot);
            cards.Add(card);
        }

        while (cards.Count > requiredCount)
        {
            int lastIndex = cards.Count - 1;
            InventorySlotCardView card = cards[lastIndex];
            cards.RemoveAt(lastIndex);

            if (card != null)
                Destroy(card.gameObject);
        }
    }

    private void RefreshLatest()
    {
        if (GameEvents.TryGetLatest<InventoryLayoutChangedEvent>(out var message))
            OnInventoryChanged(message);
    }

    public void SelectSlot(int index)
    {
        if (GameplayInputGate.IsBlocked)
            return;

        GameServices services = GameServices.Instance;
        if (services != null &&
            services.TryGet<InventoryService>(out InventoryService inventory))
        {
            inventory.SelectSlot(index);
        }
    }

    public void SwapSlots(int sourceIndex, int targetIndex)
    {
        if (GameplayInputGate.IsBlocked || sourceIndex == targetIndex)
            return;

        GameServices services = GameServices.Instance;
        if (services == null ||
            !services.TryGet<InventoryService>(out InventoryService inventory))
        {
            return;
        }

        if (!inventory.SwapSlots(sourceIndex, targetIndex))
        {
            Debug.Log("These inventory slots cannot be swapped. A parked cart's slots are locked.");
        }
    }
}
