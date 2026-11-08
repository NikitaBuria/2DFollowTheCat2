using UnityEngine;

// Coins go straight into the collector's Wallet, not the inventory.
public class CoinPickup : Pickup
{
    [SerializeField, Min(1)] private int amount = 1;

    public override bool TryPickUp(GameObject collector)
    {
        // Only things with a Wallet (the player) can collect coins; enemies walk over them.
        if (!collector.TryGetComponent(out Wallet wallet)) return false;

        wallet.Add(amount);
        return true;
    }
}
