using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Окно резьбы. Заменяет старый CarvingManager.
// Открывается по CarvingOpenedEvent, блокирует управление игроком, пока открыто.
public class CarvingPanelController : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;      // ДОЛЖЕН быть дочерним объектом, не тем же, где скрипт
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button closeButton;

    [Header("Recipes")]
    [SerializeField] private CarvingRecipeTable recipeTable;

    [Header("Pumpkin List")]
    [SerializeField] private Transform pumpkinRoot;
    [SerializeField] private CarvingPumpkinCardView pumpkinCardPrefab;

    [Header("Ornament Buttons")]
    [SerializeField] private Transform ornamentRoot;
    [SerializeField] private CarvingOrnamentButtonView ornamentButtonPrefab;
    [SerializeField] private List<Ornament> allowedOrnaments = new List<Ornament>();

    [Header("Selected Pumpkin (optional)")]
    [SerializeField] private GameObject selectedRoot;
    [SerializeField] private Image selectedIcon;
    [SerializeField] private TMP_Text selectedNameText;

    private readonly List<CarvingPumpkinCardView> pumpkinCards = new List<CarvingPumpkinCardView>();
    private readonly List<CarvingOrnamentButtonView> ornamentButtons = new List<CarvingOrnamentButtonView>();

    private int selectedSlotIndex = -1;
    private ItemDefinition selectedItem;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<CarvingOpenedEvent>(OnCarvingOpened);
        GameEvents.Subscribe<InventoryLayoutChangedEvent>(OnInventoryChanged);

        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<CarvingOpenedEvent>(OnCarvingOpened);
        GameEvents.Unsubscribe<InventoryLayoutChangedEvent>(OnInventoryChanged);

        if (closeButton != null) closeButton.onClick.RemoveListener(Close);

        GameplayInputGate.SetBlocked(false);
        IsOpen = false;
    }

    private void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    // ---------- Открытие / закрытие ----------

    private void OnCarvingOpened(CarvingOpenedEvent message)
    {
        if (panelRoot == null || pumpkinRoot == null || pumpkinCardPrefab == null ||
            ornamentRoot == null || ornamentButtonPrefab == null || recipeTable == null)
        {
            Debug.LogWarning("CarvingPanelController: заполните все поля в инспекторе.", this);
            return;
        }

        ClearSelection();
        SetMessage(string.Empty);
        if (titleText != null) titleText.text = "Carving";

        BuildOrnamentButtons();

        panelRoot.SetActive(true);
        IsOpen = true;
        GameplayInputGate.SetBlocked(true);

        RefreshAll();
    }

    private void Close()
    {
        GameplayInputGate.SetBlocked(false);
        IsOpen = false;
        ClearSelection();
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    // ---------- Реакция на инвентарь ----------

    private void OnInventoryChanged(InventoryLayoutChangedEvent message)
    {
        if (!IsOpen) return;
        RefreshAll();
    }

    // ---------- Отрисовка ----------

    private void RefreshAll()
    {
        ValidateSelection();
        RefreshPumpkins();
        RefreshSelected();
        RefreshOrnamentButtons();
    }

    private void RefreshPumpkins()
    {
        ClearPumpkinCards();

        if (!TryGetInventory(out InventoryService inventory)) return;

        for (int i = 0; i < inventory.Capacity; i++)
        {
            if (!inventory.TryGetSlotContents(i, out ItemDefinition item, out int quantity))
                continue;
            if (item == null || quantity <= 0 || !recipeTable.IsCarvable(item))
                continue;

            CarvingPumpkinCardView card = Instantiate(pumpkinCardPrefab, pumpkinRoot);
            card.Bind(i, item, quantity, i == selectedSlotIndex, OnPumpkinClicked);
            pumpkinCards.Add(card);
        }
    }

    private void RefreshSelected()
    {
        if (selectedRoot == null) return;

        bool has = selectedItem != null;
        selectedRoot.SetActive(has);
        if (!has) return;

        if (selectedIcon != null)
        {
            selectedIcon.sprite = selectedItem.Icon;
            selectedIcon.enabled = selectedIcon.sprite != null;
        }
        if (selectedNameText != null) selectedNameText.text = selectedItem.DisplayName;
    }

    private void BuildOrnamentButtons()
    {
        foreach (CarvingOrnamentButtonView button in ornamentButtons)
        {
            if (button != null) Destroy(button.gameObject);
        }
        ornamentButtons.Clear();

        foreach (Ornament ornament in allowedOrnaments)
        {
            if (ornament == null) continue;

            CarvingOrnamentButtonView view = Instantiate(ornamentButtonPrefab, ornamentRoot);
            view.Bind(ornament, OnOrnamentClicked);
            ornamentButtons.Add(view);
        }
    }

    // Кнопка активна, только если для выбранной тыквы есть рецепт
    private void RefreshOrnamentButtons()
    {
        foreach (CarvingOrnamentButtonView button in ornamentButtons)
        {
            if (button == null) continue;

            bool canApply = selectedItem != null &&
                            recipeTable.TryGetResult(selectedItem, button.Ornament, out _);
            button.SetInteractable(canApply);
        }
    }

    private void ClearPumpkinCards()
    {
        foreach (CarvingPumpkinCardView card in pumpkinCards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        pumpkinCards.Clear();
    }

    // ---------- Выбор тыквы ----------

    private void OnPumpkinClicked(int slotIndex)
    {
        if (!TryGetInventory(out InventoryService inventory)) return;

        if (!inventory.TryGetSlotContents(slotIndex, out ItemDefinition item, out int quantity) ||
            item == null || quantity <= 0)
            return;

        // Повторный клик по выбранной тыкве снимает выбор
        if (selectedSlotIndex == slotIndex && selectedItem == item)
            ClearSelection();
        else
        {
            selectedSlotIndex = slotIndex;
            selectedItem = item;
        }

        RefreshAll();
    }

    private void ClearSelection()
    {
        selectedSlotIndex = -1;
        selectedItem = null;
    }

    // Если инвентарь изменился и выбранная тыква пропала - снимаем выбор
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

    // ---------- Резьба ----------

    private void OnOrnamentClicked(Ornament ornament)
    {
        if (selectedItem == null)
        {
            SetMessage("Select a pumpkin first.");
            return;
        }
        if (!TryGetInventory(out InventoryService inventory)) return;

        ItemDefinition source = selectedItem;

        if (!recipeTable.TryGetResult(source, ornament, out ItemDefinition result))
        {
            SetMessage("This can't be applied to this pumpkin.");
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
            SetMessage("No room in inventory.");
            return;
        }

        ClearSelection();
        RefreshAll();
        SetMessage("Done: " + result.DisplayName);
    }

    // ---------- Вспомогательное ----------

    private bool TryGetInventory(out InventoryService inventory)
    {
        inventory = null;
        GameServices services = GameServices.Instance;
        if (services != null && services.TryGet(out inventory))
            return true;

        SetMessage("Player inventory is unavailable.");
        return false;
    }

    private void SetMessage(string message)
    {
        if (messageText != null) messageText.text = message;
    }
}
