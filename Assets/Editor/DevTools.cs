using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-only test shortcuts (Tools > Follow The Cat), so testing never needs cheat values in
// scripts or assets. Play-mode tools go through the game's own API (Wallet.Add, TryRestore...),
// so they behave exactly like playing; they are greyed out when not in Play mode.
public static class DevTools
{
    private const string MenuRoot = "Tools/Follow The Cat/";
    private const string ItemDatabasePath = "Assets/Scripts/ScriptableObjects/Items/ItemDatabase.asset";
    private const int CoinGift = 500;
    // Longer than any test session; Play mode ending resets it.
    private const float GodModeSeconds = 100000f;

    // ---------- Player

    [MenuItem(MenuRoot + "Player/Add 500 Coins", priority = 100)]
    private static void AddCoins()
    {
        if (!TryGetPlayer(out Player player)) return;
        player.playerWallet.Add(CoinGift);
        Debug.Log($"[Dev] +{CoinGift} coins ({player.playerWallet.coins})");
    }

    [MenuItem(MenuRoot + "Player/Give One Of Every Item", priority = 101)]
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

    [MenuItem(MenuRoot + "Player/Full Heal", priority = 120)]
    private static void FullHeal()
    {
        if (!TryGetPlayer(out Player player)) return;
        player.playerHealth.Heal(player.playerHealth.maxHealth);
        Debug.Log("[Dev] Full heal");
    }

    // Uses the normal i-frames, so nothing in Health changes. Ends when Play mode stops.
    [MenuItem(MenuRoot + "Player/God Mode (until Play stops)", priority = 121)]
    private static void GodMode()
    {
        if (!TryGetPlayer(out Player player)) return;
        player.playerHealth.StartInvincibility(GodModeSeconds);
        Debug.Log("[Dev] God mode on until Play mode stops");
    }

    // ---------- Town

    // Pays for the next building like the player would (coins + required item), then restores and saves it.
    [MenuItem(MenuRoot + "Town/Restore Next Building", priority = 200)]
    private static void RestoreNextBuilding()
    {
        if (!TryGetPlayer(out Player player) || !TryGetTown(out TownProgress town, out SaveManager saveManager)) return;

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

    [MenuItem(MenuRoot + "Town/Reset Town", priority = 201)]
    private static void ResetTown()
    {
        if (!TryGetTown(out TownProgress town, out SaveManager saveManager)) return;
        town.Load(new SaveData { restoredBuildings = new List<string>() });
        saveManager.Save();
        Debug.Log("[Dev] Town reset to ruins and saved");
    }

    [MenuItem(MenuRoot + "Player/Add 500 Coins", true)]
    [MenuItem(MenuRoot + "Player/Give One Of Every Item", true)]
    [MenuItem(MenuRoot + "Player/Full Heal", true)]
    [MenuItem(MenuRoot + "Player/God Mode (until Play stops)", true)]
    [MenuItem(MenuRoot + "Town/Restore Next Building", true)]
    [MenuItem(MenuRoot + "Town/Reset Town", true)]
    private static bool IsPlaying() => EditorApplication.isPlaying;

    // ---------- Scenes (Edit mode)

    [MenuItem(MenuRoot + "Open Scene/MainMenu", priority = 300)] private static void OpenMainMenu() => OpenScene("MainMenu");
    [MenuItem(MenuRoot + "Open Scene/Town", priority = 301)] private static void OpenTown() => OpenScene("Town");
    [MenuItem(MenuRoot + "Open Scene/Outdoor", priority = 302)] private static void OpenOutdoor() => OpenScene("Outdoor");
    [MenuItem(MenuRoot + "Open Scene/Dungeon", priority = 303)] private static void OpenDungeon() => OpenScene("Dungeon");

    [MenuItem(MenuRoot + "Open Scene/MainMenu", true)]
    [MenuItem(MenuRoot + "Open Scene/Town", true)]
    [MenuItem(MenuRoot + "Open Scene/Outdoor", true)]
    [MenuItem(MenuRoot + "Open Scene/Dungeon", true)]
    private static bool IsEditing() => !EditorApplication.isPlaying;

    // Asks to save changed scenes first (the normal Unity dialog), like double-clicking a scene.
    private static void OpenScene(string sceneName)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity");
    }

    // ---------- Helpers

    private static bool TryGetPlayer(out Player player)
    {
        player = Object.FindFirstObjectByType<Player>();
        if (player == null) Debug.LogWarning("[Dev] No Player in this scene (the MainMenu has none).");
        return player != null;
    }

    private static bool TryGetTown(out TownProgress town, out SaveManager saveManager)
    {
        town = Object.FindFirstObjectByType<TownProgress>();
        saveManager = Object.FindFirstObjectByType<SaveManager>();
        if (town == null || saveManager == null) Debug.LogWarning("[Dev] No SaveSystem (TownProgress/SaveManager) in this scene.");
        return town != null && saveManager != null;
    }
}
