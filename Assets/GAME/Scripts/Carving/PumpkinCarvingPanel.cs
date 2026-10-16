using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Окно резьбы.
// Слева - тыквы из инвентаря. Нажали на тыкву -> справа появляется меню изменения:
// сменить лицо или добавить/убрать свечу.
// Открывается по PumpkinCarvingOpenedEvent и блокирует управление игроком, пока открыто.
public class PumpkinCarvingPanel : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject panelRoot;   // ДОЛЖЕН быть дочерним объектом, не тем, где висит этот скрипт
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button closeButton;

    [Header("Left: pumpkin list")]
    [SerializeField] private Transform pumpkinListRoot;
    [SerializeField] private PumpkinCardView pumpkinCardPrefab;

    [Header("Right: edit menu")]
    [SerializeField] private GameObject editRoot;    // всё правое меню, скрыто пока тыква не выбрана
    [SerializeField] private Image selectedIcon;
    [SerializeField] private TMP_Text selectedNameText;
    [SerializeField] private Transform faceButtonsRoot;
    [SerializeField] private PumpkinFaceButtonView faceButtonPrefab;
    [SerializeField] private Button candleButton;
    [SerializeField] private TMP_Text candleButtonLabel;

    private readonly List<PumpkinCardView> pumpkinCards = new List<PumpkinCardView>();
    private readonly List<PumpkinFaceButtonView> faceButtons = new List<PumpkinFaceButtonView>();

    private PumpkinCarvingCatalog catalog;
    private int selectedSlotIndex = -1;
    private PumpkinItemDefinition selectedItem;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<PumpkinCarvingOpenedEvent>(OnCarvingOpened);
        GameEvents.Subscribe<InventoryLayoutChangedEvent>(OnInventoryChanged);

        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (candleButton != null) candleButton.onClick.AddListener(OnCandleClicked);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<PumpkinCarvingOpenedEvent>(OnCarvingOpened);
        GameEvents.Unsubscribe<InventoryLayoutChangedEvent>(OnInventoryChanged);

        if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        if (candleButton != null) candleButton.onClick.RemoveListener(OnCandleClicked);

        if (IsOpen) GameplayInputGate.SetBlocked(false);
        IsOpen = false;
    }

    private void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    // ---------- Открытие / закрытие ----------

    private void OnCarvingOpened(PumpkinCarvingOpenedEvent message)
    {
        if (message.Catalog == null || panelRoot == null || pumpkinListRoot == null ||
            pumpkinCardPrefab == null || faceButtonsRoot == null || faceButtonPrefab == null)
        {
            Debug.LogWarning("PumpkinCarvingPanel: заполните все поля в инспекторе.", this);
            return;
        }

        catalog = message.Catalog;
        ClearSelection();
        SetMessage(string.Empty);
        if (titleText != null) titleText.text = "Carving";

        BuildFaceButtons();

        panelRoot.SetActive(true);
        IsOpen = true;
        GameplayInputGate.SetBlocked(true);

        RefreshAll();
    }

    private void Close()
    {
        if (IsOpen) GameplayInputGate.SetBlocked(false);
        IsOpen = false;
        ClearSelection();
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void OnInventoryChanged(InventoryLayoutChangedEvent message)
    {
        if (!IsOpen) return;
        RefreshAll();
    }

    // ---------- Отрисовка ----------

    private void RefreshAll()
    {
        ValidateSelection();
        RefreshPumpkinList();
        RefreshEditMenu();
    }

    private void RefreshPumpkinList()
    {
        foreach (PumpkinCardView card in pumpkinCards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        pumpkinCards.Clear();

        if (!TryGetInventory(out InventoryService inventory)) return;

        for (int i = 0; i < inventory.Capacity; i++)
        {
            if (!inventory.TryGetSlotContents(i, out ItemDefinition item, out int quantity))
                continue;
            if (item == null || quantity <= 0 || !catalog.IsCarvable(item))
                continue;

            PumpkinCardView card = Instantiate(pumpkinCardPrefab, pumpkinListRoot);
            card.Bind(i, item, quantity, i == selectedSlotIndex, OnPumpkinClicked);
            pumpkinCards.Add(card);
        }
    }

    private void RefreshEditMenu()
    {
        bool hasSelection = selectedItem != null;
        if (editRoot != null) editRoot.SetActive(hasSelection);
        if (!hasSelection) return;

        if (selectedIcon != null)
        {
            selectedIcon.sprite = selectedItem.Icon;
            selectedIcon.enabled = selectedIcon.sprite != null;
        }
        if (selectedNameText != null) selectedNameText.text = selectedItem.DisplayName;

        foreach (PumpkinFaceButtonView button in faceButtons)
        {
            if (button != null) button.SetCurrent(button.Face == selectedItem.Face);
        }

        if (candleButtonLabel != null)
            candleButtonLabel.text = selectedItem.HasCandle ? "Remove candle" : "Add candle";
    }

    private void BuildFaceButtons()
    {
        foreach (PumpkinFaceButtonView button in faceButtons)
        {
            if (button != null) Destroy(button.gameObject);
        }
        faceButtons.Clear();

        foreach (PumpkinCarvingCatalog.FaceVisual visual in catalog.Faces)
        {
            // "Без лица" - это исходное состояние, в меню его не показываем
            if (visual == null || visual.face == PumpkinFace.None) continue;

            string label = string.IsNullOrEmpty(visual.label) ? visual.face.ToString() : visual.label;
            Sprite icon = visual.buttonIcon != null ? visual.buttonIcon : visual.pumpkinIcon;

            PumpkinFaceButtonView view = Instantiate(faceButtonPrefab, faceButtonsRoot);
            view.Bind(visual.face, label, icon, OnFaceClicked);
            faceButtons.Add(view);
        }
    }

    // ---------- Выбор тыквы ----------

    private void OnPumpkinClicked(int slotIndex)
    {
        if (!TryGetInventory(out InventoryService inventory)) return;

        if (!inventory.TryGetSlotContents(slotIndex, out ItemDefinition item, out int quantity) ||
            quantity <= 0 || !(item is PumpkinItemDefinition pumpkin))
            return;

        // Повторный клик по выбранной тыкве снимает выбор
        if (selectedSlotIndex == slotIndex && selectedItem == pumpkin)
            ClearSelection();
        else
        {
            selectedSlotIndex = slotIndex;
            selectedItem = pumpkin;
        }

        SetMessage(string.Empty);
        RefreshAll();
    }

    private void ClearSelection()
    {
        selectedSlotIndex = -1;
        selectedItem = null;
    }

    // Если инвентарь изменился и выбранной тыквы там уже нет - снимаем выбор
    private void ValidateSelection()
    {
        if (selectedItem == null) return;

        if (!TryGetInventory(out InventoryService inventory) ||
            !inventory.TryGetSlotContents(selectedSlotIndex, out ItemDefinition item, out int quantity) ||
            item != selectedItem || quantity <= 0)
        {
            ClearSelection();
        }
    }

    // Выбрать тыкву этого вида в инвентаре (после изменения она лежит уже в другом слоте)
    private void SelectItemInInventory(InventoryService inventory, PumpkinItemDefinition target)
    {
        for (int i = 0; i < inventory.Capacity; i++)
        {
            if (inventory.TryGetSlotContents(i, out ItemDefinition item, out int quantity) &&
                quantity > 0 && item == target)
            {
                selectedSlotIndex = i;
                selectedItem = target;
                return;
            }
        }
        ClearSelection();
    }

    // ---------- Изменение тыквы ----------

    private void OnFaceClicked(PumpkinFace face)
    {
        if (selectedItem == null) return;
        ApplyChange(face, selectedItem.HasCandle);
    }

    private void OnCandleClicked()
    {
        if (selectedItem == null) return;
        ApplyChange(selectedItem.Face, !selectedItem.HasCandle);
    }

    private void ApplyChange(PumpkinFace newFace, bool newCandle)
    {
        if (selectedItem == null || catalog == null) return;
        if (!TryGetInventory(out InventoryService inventory)) return;

        PumpkinItemDefinition source = selectedItem;

        if (!catalog.TryGetVariant(newFace, newCandle, out PumpkinItemDefinition result))
        {
            SetMessage("Variant not found. Run 'Generate Variants' on the catalog.");
            return;
        }
        if (result == source)
        {
            SetMessage("The pumpkin already looks like that.");
            return;
        }

        if (!inventory.RemoveItem(source, 1))
        {
            SetMessage("Pumpkin not found in inventory.");
            return;
        }

        // AddItem возвращает, сколько НЕ поместилось (0 = всё хорошо)
        int leftover = inventory.AddItem(result, 1);
        if (leftover > 0)
        {
            inventory.AddItem(source, 1); // возвращаем старую тыкву
            SelectItemInInventory(inventory, source);
            RefreshAll();
            SetMessage("No room in inventory.");
            return;
        }

        SelectItemInInventory(inventory, result);
        RefreshAll();
        SetMessage("Done: " + result.DisplayName);
    }

    // ---------- Вспомогательное ----------

    private bool TryGetInventory(out InventoryService inventory)
    {
        inventory = null;
        GameServices services = GameServices.Instance;
        if (services != null && services.TryGet<InventoryService>(out inventory))
            return true;

        SetMessage("Player inventory is unavailable.");
        return false;
    }

    private void SetMessage(string message)
    {
        if (messageText != null) messageText.text = message;
    }
}
