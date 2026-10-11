using System.Collections.Generic;
using UnityEngine;

public class ShopService : MonoBehaviour
{
    private readonly HashSet<string> purchasedUpgrades = new HashSet<string>();

    private void OnEnable()
    {
        GameServices services = GameServices.Instance;
        if (services == null)
        {
            Debug.LogError("GameServices is missing.", this);
            return;
        }

        if (services.Register(this))
            GameEvents.Subscribe<ShopTransactionRequestedEvent>(OnTransactionRequested);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<ShopTransactionRequestedEvent>(OnTransactionRequested);
        GameServices services = GameServices.Instance;
        if (services != null)
            services.Unregister(this);
    }

    private void OnTransactionRequested(ShopTransactionRequestedEvent request)
    {
        if (request.IsPurchase)
            TryBuy(request.Product, request.Quantity);
        else
            TrySell(request.Product, request.Quantity);
    }

    public bool TryBuy(ShopProductDefinition product, int quantity = 1)
    {
        if (product == null || !product.CanBuy)
            return Fail(product, "This product cannot be purchased.");
        if (quantity <= 0)
            return Fail(product, "Invalid purchase quantity.");
        if (!TryGetServices(out InventoryService inventory, out WalletService wallet))
            return Fail(product, "Shop services are unavailable.");

        if (product.ProductType == ShopProductType.BackpackUpgrade)
            return TryBuyBackpack(product, quantity, inventory, wallet);
        if (product.ProductType == ShopProductType.CartUpgrade)
            return TryBuyCart(product, quantity, inventory, wallet);
        if (product.Item == null)
            return Fail(product, "Product item is missing.");

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
        if (remaining > 0)
        {
            int added = quantity - remaining;
            if (added > 0)
                inventory.RemoveItem(product.Item, added);
            if (totalPrice > 0)
                wallet.AddMoney(totalPrice);
            return Fail(product, "Purchase cancelled. Inventory is full.");
        }

        PublishResult(true, product, $"Purchased {product.Item.DisplayName} x{quantity}.");
        return true;
    }

    private bool TryBuyBackpack(
        ShopProductDefinition product,
        int quantity,
        InventoryService inventory,
        WalletService wallet)
    {
        if (quantity != 1)
            return Fail(product, "Buy one backpack at a time.");
        if (string.IsNullOrWhiteSpace(product.ProductId) || product.CapacityBonus <= 0)
            return Fail(product, "Backpack product settings are invalid.");
        if (purchasedUpgrades.Contains(product.ProductId))
            return Fail(product, "You already own this upgrade.");
        if (wallet.Balance < product.BuyPrice)
            return Fail(product, "Not enough money.");
        if (product.BuyPrice > 0 && !wallet.TrySpend(product.BuyPrice))
            return Fail(product, "Payment failed.");

        if (!inventory.SetCapacityBonus(
            product.ProductId,
            product.CapacityBonus,
            InventorySlotGroup.Backpack))
        {
            if (product.BuyPrice > 0)
                wallet.AddMoney(product.BuyPrice);
            return Fail(product, "Could not apply backpack upgrade.");
        }

        purchasedUpgrades.Add(product.ProductId);
        PublishResult(true, product,
            $"Purchased {product.DisplayName}. +{product.CapacityBonus} backpack slots.");
        return true;
    }

    private bool TryBuyCart(
        ShopProductDefinition product,
        int quantity,
        InventoryService inventory,
        WalletService wallet)
    {
        if (quantity != 1)
            return Fail(product, "Buy one cart at a time.");
        if (string.IsNullOrWhiteSpace(product.ProductId) || product.CapacityBonus <= 0)
            return Fail(product, "Cart product settings are invalid.");
        if (purchasedUpgrades.Contains(product.ProductId))
            return Fail(product, "You already own this upgrade.");

        GameServices services = GameServices.Instance;
        if (services == null || !services.TryGet(out CartService cartService))
            return Fail(product, "Cart service is unavailable.");
        if (cartService.HasCart)
            return Fail(product, "You already have a cart.");
        if (wallet.Balance < product.BuyPrice)
            return Fail(product, "Not enough money.");
        if (product.BuyPrice > 0 && !wallet.TrySpend(product.BuyPrice))
            return Fail(product, "Payment failed.");

        if (!cartService.TryActivate(product.ProductId, product.CapacityBonus, inventory))
        {
            if (product.BuyPrice > 0)
                wallet.AddMoney(product.BuyPrice);
            return Fail(product, "Could not activate cart.");
        }

        purchasedUpgrades.Add(product.ProductId);
        PublishResult(true, product,
            $"Purchased {product.DisplayName}. +{product.CapacityBonus} cart slots.");
        return true;
    }

    public bool TrySell(ShopProductDefinition product, int quantity = 1)
    {
        if (product == null || !product.CanSell)
            return Fail(product, "This product cannot be sold.");
        if (quantity <= 0)
            return Fail(product, "Invalid sale quantity.");
        if (!TryGetServices(out InventoryService inventory, out WalletService wallet))
            return Fail(product, "Shop services are unavailable.");

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

        PublishResult(true, product,
            $"Sold {product.Item.DisplayName} x{quantity} for {totalPrice}.");
        return true;
    }

    private bool TryGetServices(out InventoryService inventory, out WalletService wallet)
    {
        inventory = null;
        wallet = null;
        GameServices services = GameServices.Instance;
        if (services == null)
            return false;
        return services.TryGet(out inventory) && services.TryGet(out wallet);
    }

    private bool Fail(ShopProductDefinition product, string message)
    {
        PublishResult(false, product, message);
        return false;
    }

    private void PublishResult(bool success, ShopProductDefinition product, string message)
    {
        string productId = product != null ? product.ProductId : string.Empty;
        GameEvents.Publish(new ShopTransactionEvent(success, productId, message));
        if (success)
            Debug.Log(message);
        else
            Debug.LogWarning(message);
    }
}
