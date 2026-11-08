using System;
using UnityEngine;

// One line of a loot table: what to spawn, how likely, and how many.
[Serializable]
public class LootDrop
{
    // The pickup to spawn (CoinPickup or ItemPickup prefab).
    [field: SerializeField] public GameObject prefab { get; private set; }

    // For ItemPickup prefabs: which item it carries. Leave empty for coins.
    [field: SerializeField] public ItemData item { get; private set; }

    [SerializeField, Range(0f, 1f)] private float dropChance = 1f;
    [SerializeField, Min(1)] private int minAmount = 1;
    [SerializeField, Min(1)] private int maxAmount = 1;

    public bool ShouldDrop() => prefab != null && UnityEngine.Random.value < dropChance;

    // Both ends inclusive; max is never below min.
    public int GetRandomAmount() => UnityEngine.Random.Range(minAmount, Mathf.Max(minAmount, maxAmount) + 1);
}
