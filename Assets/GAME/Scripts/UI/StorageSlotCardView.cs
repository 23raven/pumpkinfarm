using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StorageSlotCardView : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI References")]
    [SerializeField] private Image background;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text slotLabel;
    [SerializeField] private TMP_Text itemNameLabel;
    [SerializeField] private TMP_Text quantityLabel;

    [Header("Appearance")]
    [SerializeField] private Color normalColor = new Color(0.16f, 0.19f, 0.22f, 0.96f);
    [SerializeField] private Color emptyColor = new Color(0.12f, 0.14f, 0.16f, 0.82f);

    private int slotIndex;
    private StorageSlotData slotData;
    private Canvas dragCanvas;
    private StoragePanelController owner;
    private GameObject dragGhost;
    private bool isDragging;

    public int SlotIndex => slotIndex;
    public bool IsAccessible => owner != null && StoragePanelController.IsOpen;
    public StoragePanelController Owner => owner;

    private void Awake()
    {
        if (background == null) background = GetComponent<Image>();
    }

    public void Bind(int index, StorageSlotData data, Canvas canvas, StoragePanelController controller)
    {
        slotIndex = index;
        slotData = data;
        dragCanvas = canvas;
        owner = controller;

        if (slotLabel != null) slotLabel.text = $"Storage {index + 1}";
        if (itemNameLabel != null) itemNameLabel.text = data.IsEmpty ? "Empty" : data.Item.DisplayName;
        if (quantityLabel != null)
            quantityLabel.text = data.IsEmpty ? "— / —" : $"{data.Quantity} / {Mathf.Max(1, data.Item.MaxStackSize)}";

        if (itemIcon != null)
        {
            Sprite icon = data.IsEmpty ? null : data.Item.Icon;
            itemIcon.sprite = icon;
            itemIcon.enabled = icon != null;
        }

        if (background != null) background.color = data.IsEmpty ? emptyColor : normalColor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = false;
        if (!StoragePanelController.IsOpen || slotData.IsEmpty || owner == null || dragCanvas == null)
            return;

        Sprite sprite = slotData.Item != null ? slotData.Item.Icon : null;
        GameObject ghost = new GameObject("StorageDragGhost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dragGhost = ghost;
        ghost.transform.SetParent(dragCanvas.transform, false);

        Image image = ghost.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = new Color(0.2f, 0.28f, 0.35f, 0.88f);
        image.enabled = sprite != null;

        GameObject labelObject = new GameObject("ItemLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(ghost.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(4f, 4f);
        labelRect.offsetMax = new Vector2(-4f, -4f);

        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        label.text = slotData.Item.DisplayName;
        label.fontSize = 15f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        if (itemNameLabel != null && itemNameLabel.font != null)
        {
            label.font = itemNameLabel.font;
            label.fontSharedMaterial = itemNameLabel.fontSharedMaterial;
        }

        ghost.GetComponent<RectTransform>().sizeDelta = new Vector2(124f, 64f);
        isDragging = true;
        UpdateGhostPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging) UpdateGhostPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragGhost != null) Destroy(dragGhost);
        dragGhost = null;
        isDragging = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!StoragePanelController.IsOpen || owner == null || eventData.pointerDrag == null)
            return;

        InventorySlotCardView playerSource = eventData.pointerDrag.GetComponent<InventorySlotCardView>();
        if (playerSource != null)
        {
            if (playerSource.IsAccessible)
                owner.TransferFromPlayer(playerSource.SlotIndex, slotIndex);
            return;
        }

        StorageSlotCardView storageSource = eventData.pointerDrag.GetComponent<StorageSlotCardView>();
        if (storageSource != null && storageSource != this && storageSource.Owner == owner)
            owner.SwapStorageSlots(storageSource.SlotIndex, slotIndex);
    }

    public void TransferToPlayer(int playerSlotIndex)
    {
        if (StoragePanelController.IsOpen && owner != null)
            owner.TransferToPlayer(slotIndex, playerSlotIndex);
    }

    private void UpdateGhostPosition(PointerEventData eventData)
    {
        if (dragGhost == null || dragCanvas == null) return;
        RectTransform ghostRect = dragGhost.GetComponent<RectTransform>();
        RectTransform canvasRect = dragCanvas.transform as RectTransform;

        if (dragCanvas.renderMode == RenderMode.ScreenSpaceOverlay || canvasRect == null)
        {
            ghostRect.position = eventData.position;
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            ghostRect.localPosition = localPoint;
    }

    private void OnDisable()
    {
        if (dragGhost != null) Destroy(dragGhost);
        dragGhost = null;
        isDragging = false;
    }
}
