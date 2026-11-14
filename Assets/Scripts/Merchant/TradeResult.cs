// What happened when the player tried to buy or sell. Merchant.OnTrade sends it; the shop UI
// turns it into a message.
public enum TradeResult
{
    Bought,
    Sold,
    NotEnoughCoins,
    InventoryFull,
    CannotSell,  // the merchant doesn't buy this item (sellPrice 0)
    NotForSale,  // not in this merchant's stock, or no customer
}
