using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoragePanelController : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button closeButton;

    [Header("Storage Cards")]
    [SerializeField] private Transform cardRoot;
    [SerializeField] private StorageSlotCardView cardPrefab;

    private readonly List<StorageSlotCardView> cards = new List<StorageSlotCardView>();
    private Canvas rootCanvas;
    private StorageContainer currentStorage;

    private void Awake()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        rootCanvas = canvas != null ? canvas.rootCanvas : null;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<StorageOpenedEvent>(OnStorageOpened);
        GameEvents.Subscribe<StorageChangedEvent>(OnStorageChanged);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<StorageOpenedEvent>(OnStorageOpened);
        GameEvents.Unsubscribe<StorageChangedEvent>(OnStorageChanged);
        if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        GameplayInputGate.SetBlocked(false);
        IsOpen = false;
        currentStorage = null;
    }

    private void OnStorageOpened(StorageOpenedEvent message)
    {
        if (message.Container == null || panelRoot == null || cardRoot == null || cardPrefab == null)
        {
            Debug.LogWarning("StoragePanelController: Configure Panel Root, Card Root and Card Prefab.", this);
            return;
        }

        currentStorage = message.Container;
        if (titleText != null) titleText.text = currentStorage.DisplayName;
        if (messageText != null) messageText.text = "Drag items between your inventory and storage.";
        panelRoot.SetActive(true);
        IsOpen = true;
        GameplayInputGate.SetBlocked(true);
        RefreshCards();
    }

    private void OnStorageChanged(StorageChangedEvent message)
    {
        if (currentStorage == null || message.Container != currentStorage || !IsOpen)
            return;
        RefreshCards();
    }

    private void RefreshCards()
    {
        if (currentStorage == null || cardRoot == null || cardPrefab == null)
            return;

        StorageSlotData[] snapshot = currentStorage.GetSnapshot();
        EnsureCardCount(snapshot.Length);
        for (int i = 0; i < snapshot.Length; i++)
            cards[i].Bind(i, snapshot[i], rootCanvas, this);
    }

    private void EnsureCardCount(int count)
    {
        while (cards.Count < count)
            cards.Add(Instantiate(cardPrefab, cardRoot));

        while (cards.Count > count)
        {
            int last = cards.Count - 1;
            StorageSlotCardView card = cards[last];
            cards.RemoveAt(last);
            if (card != null) Destroy(card.gameObject);
        }
    }

    public void TransferFromPlayer(int playerIndex, int storageIndex)
    {
        if (!IsOpen || currentStorage == null) return;
        InventoryService inventory = GetPlayerInventory();
        if (inventory == null) return;

        if (!inventory.TryGetSlotContents(playerIndex, out ItemDefinition item, out int quantity) ||
            item == null || quantity <= 0)
            return;

        if (!currentStorage.CanAcceptAtSlot(storageIndex, item, quantity))
        {
            SetMessage("That storage slot cannot fit this stack.");
            return;
        }

        if (!inventory.TryTakeFromSlot(playerIndex, out ItemDefinition takenItem, out int takenQuantity))
            return;

        if (!currentStorage.TryAddToSlot(storageIndex, takenItem, takenQuantity))
        {
            inventory.TryAddToSlot(playerIndex, takenItem, takenQuantity);
            SetMessage("Transfer failed. The item was returned to your inventory.");
            return;
        }

        SetMessage($"Stored {takenItem.DisplayName} x{takenQuantity}.");
    }

    public void TransferToPlayer(int storageIndex, int playerIndex)
    {
        if (!IsOpen || currentStorage == null) return;
        InventoryService inventory = GetPlayerInventory();
        if (inventory == null) return;

        if (!currentStorage.TryGetSlotContents(storageIndex, out ItemDefinition item, out int quantity) ||
            item == null || quantity <= 0)
            return;

        if (!inventory.CanAcceptAtSlot(playerIndex, item, quantity))
        {
            SetMessage("That inventory slot cannot fit this stack.");
            return;
        }

        if (!currentStorage.TryTakeFromSlot(storageIndex, out ItemDefinition takenItem, out int takenQuantity))
            return;

        if (!inventory.TryAddToSlot(playerIndex, takenItem, takenQuantity))
        {
            currentStorage.TryAddToSlot(storageIndex, takenItem, takenQuantity);
            SetMessage("Transfer failed. The item was returned to storage.");
            return;
        }

        SetMessage($"Took {takenItem.DisplayName} x{takenQuantity}.");
    }

    public void SwapStorageSlots(int firstIndex, int secondIndex)
    {
        if (!IsOpen || currentStorage == null) return;
        if (!currentStorage.SwapSlots(firstIndex, secondIndex))
            SetMessage("Could not swap these storage slots.");
    }

    private InventoryService GetPlayerInventory()
    {
        GameServices services = GameServices.Instance;
        if (services != null && services.TryGet<InventoryService>(out InventoryService inventory))
            return inventory;
        SetMessage("Player inventory is unavailable.");
        return null;
    }

    private void SetMessage(string message)
    {
        if (messageText != null) messageText.text = message;
    }

    private void Close()
    {
        GameplayInputGate.SetBlocked(false);
        IsOpen = false;
        currentStorage = null;
        if (panelRoot != null) panelRoot.SetActive(false);
    }
}
