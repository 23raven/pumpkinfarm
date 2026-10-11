using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopProductRowView : MonoBehaviour
{
    [SerializeField] private TMP_Text productName;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;

    private ShopProductDefinition product;

    private void Awake()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(Buy);

        if (sellButton != null)
            sellButton.onClick.AddListener(Sell);
    }

    private void OnDestroy()
    {
        if (buyButton != null)
            buyButton.onClick.RemoveListener(Buy);

        if (sellButton != null)
            sellButton.onClick.RemoveListener(Sell);
    }

    public void Setup(ShopProductDefinition definition)
    {
        product = definition;

        if (product == null)
            return;

        if (productName != null)
        {
            productName.text = product.Item != null
                ? product.Item.DisplayName
                : product.name;
        }

        if (buyButton != null)
        {
            buyButton.interactable = product.CanBuy;

            TMP_Text label = buyButton.GetComponentInChildren<TMP_Text>();

            if (label != null)
                label.text = $"Buy ({product.BuyPrice})";
        }

        if (sellButton != null)
        {
            sellButton.interactable = product.CanSell;

            TMP_Text label = sellButton.GetComponentInChildren<TMP_Text>();

            if (label != null)
                label.text = $"Sell ({product.SellPrice})";
        }
    }

    private void Buy()
    {
        if (product == null)
            return;

        GameEvents.Publish(
            new ShopTransactionRequestedEvent(product, 1, true)
        );
    }

    private void Sell()
    {
        if (product == null)
            return;

        GameEvents.Publish(
            new ShopTransactionRequestedEvent(product, 1, false)
        );
    }
}