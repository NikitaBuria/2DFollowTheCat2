using System;
using UnityEngine;

// The player's coins. Earned from loot and selling, spent on town restoration.
// UI and other systems listen to OnMoneyChanged instead of polling.
public class Wallet : MonoBehaviour, ISaveable
{
    public int coins { get; private set; }

    // Sends the new total.
    public event Action<int> OnMoneyChanged;

    public void Add(int amount)
    {
        if (amount <= 0) return;

        coins += amount;
        Debug.Log($"{name} got {amount} coins. Total: {coins}.", this);
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

    // Death penalty: loses this share of the coins (rounded down). Returns how many were lost.
    public int LoseShare(float share)
    {
        int lost = Mathf.FloorToInt(coins * Mathf.Clamp01(share));
        if (lost <= 0) return 0;

        coins -= lost;
        OnMoneyChanged?.Invoke(coins);
        return lost;
    }

    public void Save(SaveData data) => data.coins = coins;

    public string Validate(SaveData data) => data.coins < 0 ? "negative coins" : null;

    public void Load(SaveData data)
    {
        coins = data.coins;
        OnMoneyChanged?.Invoke(coins);
    }
}
