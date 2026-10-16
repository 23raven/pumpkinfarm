using System;
using UnityEngine;

public class BuyPumpkinSeedsCondition : TutorialCondition
{
    [Header("Shop")]
    [SerializeField] private ShopCatalog targetShop;

    [Header("Product")]
    [SerializeField] private ShopProductDefinition pumpkinSeedsProduct;

    private TutorialStep activeStep;
    private ShopCatalog openedCatalog;
    private bool purchased;

    private void OnEnable()
    {
        GameEvents.Subscribe<ShopOpenedEvent>(OnShopOpened);
        GameEvents.Subscribe<ShopTransactionEvent>(OnShopTransaction);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<ShopOpenedEvent>(OnShopOpened);
        GameEvents.Unsubscribe<ShopTransactionEvent>(OnShopTransaction);
    }

    public override bool IsMet(TutorialStep step)
    {
        if (step == null || !step.isActive)
            return false;

        if (activeStep != step)
        {
            activeStep = step;
            openedCatalog = null;
            purchased = false;
        }

        return purchased;
    }

    private void OnShopOpened(ShopOpenedEvent message)
    {
        if (activeStep == null || !activeStep.isActive)
            return;

        openedCatalog = message.Catalog;
    }

    private void OnShopTransaction(ShopTransactionEvent message)
    {
        if (activeStep == null ||
            !activeStep.isActive ||
            targetShop == null ||
            pumpkinSeedsProduct == null)
        {
            return;
        }

        if (openedCatalog != targetShop)
            return;

        if (!message.Success ||
            message.ProductId != pumpkinSeedsProduct.ProductId)
        {
            return;
        }

        // ShopTransactionEvent is shared by purchases and sales.
        // Only accept a successful purchase.
        if (string.IsNullOrEmpty(message.Message) ||
            !message.Message.StartsWith(
                "Purchased ",
                StringComparison.Ordinal))
        {
            return;
        }

        if (pumpkinSeedsProduct.ProductType != ShopProductType.Item ||
            pumpkinSeedsProduct.Item == null)
        {
            Debug.LogWarning(
                "BuyPumpkinSeedsCondition: The assigned product must be an item.",
                this);
            return;
        }

        purchased = true;
    }
}