using System;
using UnityEngine;

/// <summary>
/// Sends one fixed tutorial email when this GameObject is first activated.
/// Keep the GameObject inactive until the relevant TutorialStep begins.
/// </summary>
public class TutorialPurchaseMailSender : MonoBehaviour
{
    public const string MessageId = "tutorial-buy-pumpkin";

    private const string MessageSubject = "Your First Shopping Task";

    [Header("Required Store Product")]
    [SerializeField] private ShopCatalog targetShop;
    [SerializeField] private ShopProductDefinition pumpkinProduct;

    private bool sendAttempted;

    private void Start()
    {
        SendTutorialMail();
    }

    private void SendTutorialMail()
    {
        if (sendAttempted)
            return;

        sendAttempted = true;

        if (targetShop == null)
        {
            Debug.LogError("TutorialPurchaseMailSender: Target Shop is not assigned.", this);
            return;
        }

        if (pumpkinProduct == null || pumpkinProduct.Item == null)
        {
            Debug.LogError("TutorialPurchaseMailSender: Pumpkin Product or its Item is not assigned.", this);
            return;
        }

        if (pumpkinProduct.ProductType != ShopProductType.Item || !pumpkinProduct.CanBuy)
        {
            Debug.LogError("TutorialPurchaseMailSender: The assigned pumpkin product must be a purchasable item.", this);
            return;
        }

        bool productBelongsToShop = false;
        ShopProductDefinition[] products = targetShop.Products;

        if (products != null)
        {
            foreach (ShopProductDefinition product in products)
            {
                if (product == pumpkinProduct)
                {
                    productBelongsToShop = true;
                    break;
                }
            }
        }

        if (!productBelongsToShop)
        {
            Debug.LogError(
                "TutorialPurchaseMailSender: Pumpkin Product must be included in the selected Shop Catalog.",
                this);
            return;
        }

        GameServices services = GameServices.Instance;
        if (services == null ||
            !services.TryGet<MailboxService>(out MailboxService mailbox))
        {
            Debug.LogError("TutorialPurchaseMailSender: MailboxService is unavailable.", this);
            return;
        }

        int day = 1;
        if (services.TryGet<DayCycleService>(out DayCycleService dayCycle))
            day = Mathf.Max(1, dayCycle.CurrentDay);

        string itemName = pumpkinProduct.Item.DisplayName;
        string body = $"Please buy 1 {itemName} at {targetShop.DisplayName}.";

        mailbox.Receive(new MailMessageData(
            MessageId,
            day,
            MessageSubject,
            body
        ));
    }
}
