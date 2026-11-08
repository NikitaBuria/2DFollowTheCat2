using System;
using UnityEngine;

// The player's coins. Earned from loot and selling, spent on town restoration.
// UI and other systems listen to OnMoneyChanged instead of polling.
public class Wallet : MonoBehaviour
{
    public int coins { get; private set; }

    // Sends the new total.
    public event Action<int> OnMoneyChanged;

    public void Add(int amount)
    {
        if (amount <= 0) return;

        coins += amount;
        Debug.Log($"{name} picked up {amount} coins. Total: {coins}.", this);
        OnMoneyChanged?.Invoke(coins);
    }

    // Spends only if there is enough; returns whether the purchase happened.
    public bool TrySpend(int amount)
    {
        if (amount <= 0 || coins < amount) return false;

        coins -= amount;
        OnMoneyChanged?.Invoke(coins);
        return true;
    }
}
