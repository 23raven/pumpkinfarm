using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotCardView : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    [Header("UI References")]
    [SerializeField] private Image background;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text groupLabel;
    [SerializeField] private TMP_Text itemNameLabel;
    [SerializeField] private TMP_Text quantityLabel;
    [SerializeField] private Image selectionBorder;

    [Header("Appearance")]
    [SerializeField] private Color normalColor = new Color(0.16f, 0.19f, 0.22f, 0.96f);
    [SerializeField] private Color selectedColor = new Color(0.25f, 0.34f, 0.43f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.1f, 0.11f, 0.12f, 0.88f);
    [SerializeField] private Color selectedBorderColor = new Color(1f, 0.78f, 0.25f, 1f);
    [SerializeField, Min(1f)] private float selectedScale = 1.07f;

    private int slotIndex;
    private InventorySlotDisplayData slotData;
    private bool isSelected;
    private bool isAccessible;
    private InventoryHUDController owner;
    private Canvas dragCanvas;
    private GameObject dragGhost;
    private bool isDragging;
    private Vector3 baseScale;

    public int SlotIndex => slotIndex;
    public bool IsAccessible => isAccessible;

    private void Awake()
    {
        if (background == null)
            background = GetComponent<Image>();

        baseScale = transform.localScale;
    }

    public void Bind(
        int index,
        InventorySlotDisplayData data,
        bool selected,
        string groupName,
        Canvas canvas,
        InventoryHUDController controller)
    {
        slotIndex = index;
        slotData = data;
        isSelected = selected;
        isAccessible = data.IsAccessible;
        owner = controller;
        dragCanvas = canvas;

        if (groupLabel != null)
        {
            groupLabel.text = groupName;
            if (data.Group == InventorySlotGroup.Cart && !data.IsAccessible)
                groupLabel.text += " • PARKED";
        }

        if (itemNameLabel != null)
        {
            if (data.IsEmpty)
                itemNameLabel.text = data.IsAccessible ? "Empty" : "Locked";
            else
                itemNameLabel.text = data.Item.DisplayName +
                    (!data.IsAccessible ? " (stored)" : string.Empty);
        }

        if (quantityLabel != null)
        {
            quantityLabel.text = data.IsEmpty
                ? "— / —"
                : $"{data.Quantity} / {Mathf.Max(1, data.Item.MaxStackSize)}";

            if (!data.IsAccessible && !data.IsEmpty)
                quantityLabel.text += "  ·  Stored";
        }

        if (itemIcon != null)
        {
            Sprite icon = data.IsEmpty ? null : data.Item.Icon;
            itemIcon.sprite = icon;
            itemIcon.enabled = icon != null;
        }

        if (background != null)
            background.color = !data.IsAccessible
                ? lockedColor
                : selected ? selectedColor : normalColor;

        if (selectionBorder != null)
        {
            selectionBorder.enabled = selected;
            selectionBorder.color = selectedBorderColor;
        }

        transform.localScale = baseScale * (selected ? selectedScale : 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left ||
            !isAccessible || isDragging || owner == null)
        {
            return;
        }

        owner.SelectSlot(slotIndex);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (GameplayInputGate.IsBlocked || !isAccessible || slotData.IsEmpty ||
            owner == null || dragCanvas == null)
        {
            return;
        }

        Sprite sprite = slotData.Item != null ? slotData.Item.Icon : null;
        if (sprite == null && itemIcon != null)
            sprite = itemIcon.sprite;
        if (sprite == null && background != null)
            sprite = background.sprite;

        GameObject ghost = new GameObject(
            "InventoryDragGhost",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        dragGhost = ghost;
        ghost.transform.SetParent(dragCanvas.transform, false);

        Image ghostImage = ghost.GetComponent<Image>();
        ghostImage.sprite = sprite;
        ghostImage.preserveAspect = true;
        ghostImage.raycastTarget = false;
        ghostImage.color = new Color(0.15f, 0.18f, 0.22f, 0.9f);
        ghostImage.enabled = sprite != null;

        RectTransform ghostRect = ghost.GetComponent<RectTransform>();
        ghostRect.sizeDelta = new Vector2(124f, 64f);

        GameObject labelObject = new GameObject(
            "ItemLabel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        labelObject.transform.SetParent(ghost.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(4f, 4f);
        labelRect.offsetMax = new Vector2(-4f, -4f);

        TMP_Text ghostLabel = labelObject.GetComponent<TMP_Text>();
        if (itemNameLabel != null && itemNameLabel.font != null)
        {
            ghostLabel.font = itemNameLabel.font;
            ghostLabel.fontSharedMaterial = itemNameLabel.fontSharedMaterial;
        }
        ghostLabel.text = slotData.Item.DisplayName;
        ghostLabel.fontSize = 16f;
        ghostLabel.color = Color.white;
        ghostLabel.alignment = TextAlignmentOptions.Center;
        ghostLabel.raycastTarget = false;

        isDragging = true;
        UpdateGhostPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
            UpdateGhostPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragGhost != null)
            Destroy(dragGhost);

        dragGhost = null;
        isDragging = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!isAccessible || owner == null || eventData.pointerDrag == null)
            return;

        InventorySlotCardView source =
            eventData.pointerDrag.GetComponent<InventorySlotCardView>();

        if (source == null || source == this || source.owner != owner ||
            !source.IsAccessible)
        {
            return;
        }

        owner.SwapSlots(source.SlotIndex, slotIndex);
    }

    private void UpdateGhostPosition(PointerEventData eventData)
    {
        if (dragGhost == null || dragCanvas == null)
            return;

        RectTransform ghostRect = dragGhost.GetComponent<RectTransform>();
        RectTransform canvasRect = dragCanvas.transform as RectTransform;

        if (dragCanvas.renderMode == RenderMode.ScreenSpaceOverlay || canvasRect == null)
        {
            ghostRect.position = eventData.position;
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            ghostRect.localPosition = localPoint;
        }
    }

    private void OnDisable()
    {
        if (dragGhost != null)
            Destroy(dragGhost);

        dragGhost = null;
        isDragging = false;
    }
}
