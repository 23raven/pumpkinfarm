using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button closeButton;

    [Header("Products")]
    [SerializeField] private Transform productListRoot;
    [SerializeField] private ShopProductRowView rowPrefab;

    private readonly List<ShopProductRowView> rows =
        new List<ShopProductRowView>();

    private void OnEnable()
    {
        GameEvents.Subscribe<ShopOpenedEvent>(OnShopOpened);
        GameEvents.Subscribe<ShopTransactionEvent>(OnTransactionResult);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<ShopOpenedEvent>(OnShopOpened);
        GameEvents.Unsubscribe<ShopTransactionEvent>(OnTransactionResult);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        GameplayInputGate.SetBlocked(false);
    }

    private void Update()
    {
        if (panelRoot != null &&
            panelRoot.activeSelf &&
            Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    private void OnShopOpened(ShopOpenedEvent message)
    {

        if (message.Catalog == null ||
            panelRoot == null ||
            productListRoot == null ||
            rowPrefab == null)
        {
            Debug.LogWarning(
                "ShopPanelController: Shop UI is not fully configured.",
                this
            );
            return;
        }

        ClearRows();

        if (titleText != null)
            titleText.text = message.Catalog.DisplayName;

        if (messageText != null)
            messageText.text = string.Empty;

        if (message.Catalog.Products != null)
        {
            foreach (ShopProductDefinition product
                     in message.Catalog.Products)
            {
                if (product == null)
                    continue;

                ShopProductRowView row =
                    Instantiate(rowPrefab, productListRoot);

                row.Setup(product);
                rows.Add(row);
            }
        }

        panelRoot.SetActive(true);
        GameplayInputGate.SetBlocked(true);
    }

    private void OnTransactionResult(ShopTransactionEvent message)
    {
        if (messageText != null && panelRoot != null && panelRoot.activeSelf)
            messageText.text = message.Message;
    }

    private void Close()
    {
        GameplayInputGate.SetBlocked(false);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void ClearRows()
    {
        foreach (ShopProductRowView row in rows)
        {
            if (row != null)
                Destroy(row.gameObject);
        }

        rows.Clear();
    }
}