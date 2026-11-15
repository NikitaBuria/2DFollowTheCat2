using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Editor-only test shortcuts for the player (Tools > Follow The Cat > Player), so testing never needs
// cheat values in scripts or assets. They go through the game's own API (Wallet.Add, Inventory, Health),
// so they behave exactly like playing, and are greyed out when not in Play mode.
public static class DevTools
{
    private const string MenuRoot = "Tools/Follow The Cat/Player/";
    private const string TownMenu = "Tools/Follow The Cat/Town/";
    private const string ItemDatabasePath = "Assets/Scripts/ScriptableObjects/Items/ItemDatabase.asset";
    private const int CoinGift = 500;
    // Longer than any test session; Play mode ending resets it.
    private const float GodModeSeconds = 100000f;

    [MenuItem(MenuRoot + "Add 500 Coins", priority = 100)]
    private static void AddCoins()
    {
        if (!TryGetPlayer(out Player player)) return;
        player.playerWallet.Add(CoinGift);
        Debug.Log($"[Dev] +{CoinGift} coins ({player.playerWallet.coins})");
    }

    [MenuItem(MenuRoot + "Give One Of Every Item", priority = 101)]
    private static void GiveEveryItem()
    {
        if (!TryGetPlayer(out Player player)) return;

        var database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(ItemDatabasePath);
        var items = new SerializedObject(database).FindProperty("items");
        var missed = new List<string>();
        for (int i = 0; i < items.arraySize; i++)
        {
            var item = items.GetArrayElementAtIndex(i).objectReferenceValue as ItemData;
            if (item == null) continue;
            if (player.playerInventory.AddItemAndReturnAccepted(item, 1) == 0) missed.Add(item.displayName);
        }
        Debug.Log(missed.Count == 0 ? "[Dev] Gave one of every item" : "[Dev] Inventory full, missed: " + string.Join(", ", missed));
    }

    [MenuItem(MenuRoot + "Full Heal", priority = 120)]
    private static void FullHeal()
    {
        if (!TryGetPlayer(out Player player)) return;
        player.playerHealth.Heal(player.playerHealth.maxHealth);
        Debug.Log("[Dev] Full heal");
    }

    // Uses the normal i-frames, so nothing in Health changes. Ends when Play mode stops.
    [MenuItem(MenuRoot + "God Mode (until Play stops)", priority = 121)]
    private static void GodMode()
    {
        if (!TryGetPlayer(out Player player)) return;
        player.playerHealth.StartInvincibility(GodModeSeconds);
        Debug.Log("[Dev] God mode on until Play mode stops");
    }

    // Pays for the next building like the player would (coins + required item), then restores and saves it.
    [MenuItem(TownMenu + "Restore Next Building", priority = 200)]
    private static void RestoreNextBuilding()
    {
        if (!TryGetPlayer(out Player player)) return;
        var town = Object.FindFirstObjectByType<TownProgress>();
        var saveManager = Object.FindFirstObjectByType<SaveManager>();
        if (town == null || saveManager == null) { Debug.LogWarning("[Dev] No SaveSystem (TownProgress/SaveManager) in this scene."); return; }

        var buildings = new SerializedObject(town).FindProperty("buildings");
        for (int i = 0; i < buildings.arraySize; i++)
        {
            var building = buildings.GetArrayElementAtIndex(i).objectReferenceValue as BuildingData;
            if (building == null || town.IsRestored(building)) continue;

            player.playerWallet.Add(building.cost);
            if (building.requiredItem != null) player.playerInventory.AddItemAndReturnAccepted(building.requiredItem, 1);

            if (town.TryRestore(building, player))
            {
                saveManager.Save();
                Debug.Log($"[Dev] Restored {building.buildingName} (tier {town.townTier})");
            }
            else
            {
                Debug.LogWarning($"[Dev] Could not restore {building.buildingName}: {town.GetRestoreBlock(building, player)}");
            }
            return;
        }
        Debug.Log("[Dev] Every building is already restored");
    }

    [MenuItem(MenuRoot + "Add 500 Coins", true)]
    [MenuItem(MenuRoot + "Give One Of Every Item", true)]
    [MenuItem(MenuRoot + "Full Heal", true)]
    [MenuItem(MenuRoot + "God Mode (until Play stops)", true)]
    [MenuItem(TownMenu + "Restore Next Building", true)]
    private static bool IsPlaying() => EditorApplication.isPlaying;

    private static bool TryGetPlayer(out Player player)
    {
        player = Object.FindFirstObjectByType<Player>();
        if (player == null) Debug.LogWarning("[Dev] No Player in this scene (the MainMenu has none).");
        return player != null;
    }
}
