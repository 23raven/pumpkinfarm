using UnityEngine;

public class ShopService : MonoBehaviour
{
    private void OnEnable()
    {
        GameServices services = GameServices.Instance;

        if (services == null)
        {
            Debug.LogError("GameServices is missing.", this);
            return;
        }

        if (services.Register(this))
        {
            GameEvents.Subscribe<ShopTransactionRequestedEvent>(
                OnTransactionRequested
            );
        }
    }

    private void OnDisable()
    {
        GameServices services = GameServices.Instance;

        if (services != null)
            services.Unregister(this);

        GameEvents.Unsubscribe<ShopTransactionRequestedEvent>(
        OnTransactionRequested
        );
    }

    public bool TryBuy(
        ShopProductDefinition product,
        int quantity = 1)
    {
        if (product == null || !product.CanBuy)
            return Fail(product, "This product cannot be purchased.");

        if (quantity <= 0)
            return Fail(product, "Invalid purchase quantity.");

        if (!TryGetServices(
            out InventoryService inventory,
            out WalletService wallet))
        {
            return Fail(product, "Shop services are unavailable.");
        }

        long priceLong = (long)product.BuyPrice * quantity;

        if (priceLong > int.MaxValue)
            return Fail(product, "Purchase price is too high.");

        int totalPrice = (int)priceLong;

        if (inventory.GetAvailableSpace(product.Item) < quantity)
            return Fail(product, "Not enough inventory space.");

        if (wallet.Balance < totalPrice)
            return Fail(product, "Not enough money.");

        if (totalPrice > 0 && !wallet.TrySpend(totalPrice))
            return Fail(product, "Payment failed.");

        int remaining = inventory.AddItem(product.Item, quantity);

        // Refund if the inventory could not accept the entire purchase.
        if (remaining > 0)
        {
            int added = quantity - remaining;

            if (added > 0)
                inventory.RemoveItem(product.Item, added);

            if (totalPrice > 0)
                wallet.AddMoney(totalPrice);

            return Fail(product, "Purchase cancelled. Inventory is full.");
        }

        string message =
            $"Purchased {product.Item.DisplayName} x{quantity}.";

        PublishResult(true, product, message);
        return true;
    }

    public bool TrySell(
        ShopProductDefinition product,
        int quantity = 1)
    {
        if (product == null || !product.CanSell)
            return Fail(product, "This product cannot be sold.");

        if (quantity <= 0)
            return Fail(product, "Invalid sale quantity.");

        if (!TryGetServices(
            out InventoryService inventory,
            out WalletService wallet))
        {
            return Fail(product, "Shop services are unavailable.");
        }

        long priceLong = (long)product.SellPrice * quantity;

        if (priceLong > int.MaxValue)
            return Fail(product, "Sale price is too high.");

        if (!inventory.HasItem(product.Item, quantity))
            return Fail(product, "Not enough items to sell.");

        if (!inventory.RemoveItem(product.Item, quantity))
            return Fail(product, "Sale failed.");

        int totalPrice = (int)priceLong;

        if (totalPrice > 0)
            wallet.AddMoney(totalPrice);

        string message =
            $"Sold {product.Item.DisplayName} x{quantity} for {totalPrice}.";

        PublishResult(true, product, message);
        return true;
    }

    private bool TryGetServices(
        out InventoryService inventory,
        out WalletService wallet)
    {
        inventory = null;
        wallet = null;

        GameServices services = GameServices.Instance;

        if (services == null)
            return false;

        return services.TryGet(out inventory) &&
               services.TryGet(out wallet);
    }

    private bool Fail(
        ShopProductDefinition product,
        string message)
    {
        PublishResult(false, product, message);
        return false;
    }

    private void PublishResult(
        bool success,
        ShopProductDefinition product,
        string message)
    {
        string productId = product != null
            ? product.ProductId
            : string.Empty;

        GameEvents.Publish(
            new ShopTransactionEvent(success, productId, message)
        );

        if (success)
            Debug.Log(message);
        else
            Debug.LogWarning(message);
    }

    private void OnTransactionRequested(
    ShopTransactionRequestedEvent request)
    {
        if (request.IsPurchase)
            TryBuy(request.Product, request.Quantity);
        else
            TrySell(request.Product, request.Quantity);
    }
}