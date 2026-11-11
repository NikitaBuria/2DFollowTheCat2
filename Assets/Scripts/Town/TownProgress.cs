using System;
using System.Collections.Generic;
using UnityEngine;

// Which town buildings are restored, and the town tier that follows from it.
// On the SaveSystem object (part of Core, so every scene has one); saved through ISaveable.
// Buildings, the merchant's look and his stock only listen to OnProgressChanged.
public class TownProgress : MonoBehaviour, ISaveable
{
    [Tooltip("Every building that can be restored. Saved IDs are checked against this list.")]
    [SerializeField] private BuildingData[] buildings;
    [Tooltip("Highest town tier (the merchant has 3 looks). Tier = 1 + restored buildings, capped here.")]
    [SerializeField, Min(1)] private int maxTier = 3;

    private readonly HashSet<string> restoredIds = new();

    public int townTier => Mathf.Min(1 + restoredIds.Count, maxTier);

    public event Action OnProgressChanged;

    public bool IsRestored(BuildingData building) => building != null && restoredIds.Contains(building.id);

    // RestoreBlock.None when the player can restore it now, otherwise why not.
    public RestoreBlock GetRestoreBlock(BuildingData building, Player player)
    {
        if (IsRestored(building)) return RestoreBlock.AlreadyRestored;
        if (building.requiredItem != null && !player.playerInventory.HasItem(building.requiredItem))
            return RestoreBlock.MissingItem;
        if (player.playerWallet.coins < building.cost) return RestoreBlock.NotEnoughCoins;

        return RestoreBlock.None;
    }

    // Checks everything first, then pays, so a refused restore changes nothing.
    public bool TryRestore(BuildingData building, Player player)
    {
        if (building == null || GetRestoreBlock(building, player) != RestoreBlock.None) return false;

        player.playerWallet.TrySpend(building.cost); // can't fail: the coins were checked
        if (building.requiredItem != null) player.playerInventory.ConsumeItem(building.requiredItem);

        restoredIds.Add(building.id);
        OnProgressChanged?.Invoke();
        return true;
    }

    public void Save(SaveData data) => data.restoredBuildings = new List<string>(restoredIds);

    public string Validate(SaveData data)
    {
        if (data.restoredBuildings == null) return null; // saves from before restoration existed

        foreach (string id in data.restoredBuildings)
        {
            if (Array.Find(buildings, b => b != null && b.id == id) == null)
                return $"unknown building '{id}' (missing from TownProgress.buildings?)";
        }

        return null;
    }

    public void Load(SaveData data)
    {
        restoredIds.Clear();
        if (data.restoredBuildings != null) restoredIds.UnionWith(data.restoredBuildings);

        OnProgressChanged?.Invoke();
    }
}
