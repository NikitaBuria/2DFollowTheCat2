using System;
using UnityEngine;

// Coins go straight into the collector's Wallet, not the inventory.
public class CoinPickup : Pickup
{
    [SerializeField, Min(1)] private int amount = 1;

    // Any coin picked up anywhere (the collector, the amount). Static because the pickup is destroyed
    // right after; PlayerSoundController listens. Selling adds coins too but is not a pickup.
    public static event Action<GameObject, int> OnAnyCoinCollected;

    public override bool TryPickUp(GameObject collector)
    {
        // Only things with a Wallet (the player) can collect coins; enemies walk over them.
        if (!collector.TryGetComponent(out Wallet wallet)) return false;

        wallet.Add(amount);
        OnAnyCoinCollected?.Invoke(collector, amount);
        return true;
    }
}
