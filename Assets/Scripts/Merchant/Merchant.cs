using System;
using UnityEngine;

// The merchant NPC. Interacting only asks GameStateManager for the Shop state; it never touches
// the UI. UIManager shows the merchant and inventory panels, and the menu or inventory key closes the shop.
// Buying and selling happen here; the merchant has unlimited coins and unlimited stock.
// What he sells grows with the town tier (one ShopStock per tier).
// Every buy or sell attempt fires OnTrade with the result, so the shop UI can say what happened.
// PlayerInteractor finds interactables through their collider.
[RequireComponent(typeof(Collider2D))]
public class Merchant : MonoBehaviour, IInteractable
{
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private string tradePrompt = "Trade";
    [Tooltip("Stock for town tier 1, 2, 3... The last one is used for higher tiers.")]
    [SerializeField] private ShopStock[] stockByTier;
    [SerializeField] private TownProgress townProgress;
    [Tooltip("Greeting said when the shop opens. Sits on a child without a collider, so the Merchant stays this object's only IInteractable.")]
    [SerializeField] private NpcDialogue greeting;

    public ShopStock stock => stockByTier[Mathf.Clamp(townProgress.townTier - 1, 0, stockByTier.Length - 1)];

    // Every buy/sell attempt: the result, the item, and how many items/coins were involved.
    public event Action<TradeResult, ItemData, int> OnTrade;

    // The player who opened the shop; the shop UI trades with them.
    public Player customer { get; private set; }

    public bool CanInteract(Player player) => true;

    public string GetPrompt(Player player) => tradePrompt;

    public void Interact(Player player)
    {
        customer = player;
        if (greeting != null) greeting.Interact(player);
        gameStateManager.ChangeState(GameState.Shop);
    }

    // Sells the whole stack in this inventory slot.
    public TradeResult TrySell(int slotIndex)
    {
        if (customer == null) return Report(TradeResult.NotForSale, null, 0);

        InventorySlot slot = customer.playerInventory.GetSlot(slotIndex);
        if (slot == null || slot.isEmpty) return Report(TradeResult.NotForSale, null, 0);

        // sellPrice 0 = can't be sold (starting Lantern, quest items).
        if (slot.item.sellPrice <= 0) return Report(TradeResult.CannotSell, slot.item, 0);

        // Read item and price before the stack is removed; afterwards slot.item is null.
        ItemData item = slot.item;
        int price = item.sellPrice * slot.amount;

        customer.playerInventory.DeleteSlot(slotIndex);
        customer.playerWallet.Add(price);

        return Report(TradeResult.Sold, item, price);
    }

    // Buys one of this item.
    public TradeResult TryBuy(ItemData item)
    {
        // Don't trust the UI: only items this merchant actually sells.
        if (customer == null || item == null || !stock.HasItem(item)) return Report(TradeResult.NotForSale, item, 0);

        // Check the coins first (read-only), so nothing has changed if the player can't pay.
        if (customer.playerWallet.coins < item.buyPrice) return Report(TradeResult.NotEnoughCoins, item, item.buyPrice);

        // Adds the item; 0 accepted = inventory full, and still nothing has changed.
        if (customer.playerInventory.AddItemAndReturnAccepted(item, 1) == 0) return Report(TradeResult.InventoryFull, item, 0);

        // Can't fail now: the coins were checked above.
        customer.playerWallet.TrySpend(item.buyPrice);
        return Report(TradeResult.Bought, item, item.buyPrice);
    }

    private TradeResult Report(TradeResult result, ItemData item, int coins)
    {
        OnTrade?.Invoke(result, item, coins);
        return result;
    }
}
