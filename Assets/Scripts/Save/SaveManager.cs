using System;
using System.IO;
using UnityEngine;

// Saves and loads the player's ISaveable components (Health, Wallet, Inventory, Equipment) and the ones on
// this object (TownProgress) through SaveFile.
// Loading validates everything first and falls back to the .bak file, so a broken save never half-loads.
// Ported from MetrooDvania's checkpoint save, without the respawn, loot and debug parts.
public class SaveManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private string saveFileName = SaveFile.DefaultFileName;
    [Tooltip("Load the save file when the scene starts. Turn off to test from a fresh start.")]
    [SerializeField] private bool loadOnStart = true;

    private ISaveable[] saveables;
    private string savePath => SaveFile.GetPath(saveFileName);

    // Set when the save file comes from a newer version of the game: saving would overwrite it with older data.
    private bool newerSaveFound;

    // False while saving is refused on purpose (dead player, newer save on disk); Exit leaves without saving then.
    public bool canSave => player.playerHealth.isAlive && !newerSaveFound;

    private void Awake()
    {
        ISaveable[] playerSaveables = player.GetComponents<ISaveable>();
        ISaveable[] ownSaveables = GetComponents<ISaveable>();
        saveables = new ISaveable[playerSaveables.Length + ownSaveables.Length];
        playerSaveables.CopyTo(saveables, 0);
        ownSaveables.CopyTo(saveables, playerSaveables.Length);
    }

    // Start, not Awake: Player.Awake has set max health and Inventory.Awake has made its slots.
    private void Start()
    {
        if (loadOnStart) Load();
    }

    // Debug keys (F5/F9) through PlayerInputReader. The game also saves on its own: doors, pause-menu Exit,
    // restoring a building, after a respawn and when the window closes.
    private void Update()
    {
        if (player.playerInputReader.savePressedThisFrame) Save();
        if (player.playerInputReader.loadPressedThisFrame) ReloadSavedGame();
    }

    // F9: back to the saved scene and position; that scene's SaveManager then loads the rest in Start.
    // Saves without a usable location (older than v5, or a scene no longer in the build) load in place.
    public void ReloadSavedGame()
    {
        if (!player.playerHealth.isAlive) return;

        SaveData data = ReadValidSave(out _);
        if (data == null) return;

        if (SceneArrival.HasValidLocation(data)) SceneLoader.Load(data.sceneName, data.position);
        else Apply(data, false);
    }

    // Closing the window (Alt+F4, the X) skips the pause menu's Exit, so save here too.
    private void OnApplicationQuit()
    {
        Save();
    }

    public bool Save()
    {
        // Never save a dead player (the save would be useless and Validate would reject it),
        // and never overwrite a save from a newer version of the game.
        if (!canSave) return false;

        SaveData data = new();
        try
        {
            foreach (ISaveable saveable in saveables)
                saveable.Save(data);
        }
        catch (Exception e)
        {
            // One broken component must not stop the caller (Exit, a door) halfway; the old save stays.
            Debug.LogException(e, this);
            return false;
        }

        if (!SaveFile.Write(data, savePath, out string error))
        {
            Debug.LogWarning($"Save failed: {error}", this);
            return false;
        }

        Debug.Log($"Game saved to {savePath}", this);
        return true;
    }

    public bool Load()
    {
        // Dying is handled by RespawnManager; loading over the Dead state would leave the player stuck.
        if (!player.playerHealth.isAlive) return false;

        SaveData data = ReadValidSave(out bool fromBackup);
        if (data == null) return false;

        Apply(data, fromBackup);
        return true;
    }

    private void Apply(SaveData data, bool fromBackup)
    {
        foreach (ISaveable saveable in saveables)
            saveable.Load(data);

        Debug.Log(fromBackup ? "Game loaded from the backup save." : "Game loaded.", this);
    }

    // The main save, or the .bak when the main one is missing, unreadable or rejected; null when neither works.
    private SaveData ReadValidSave(out bool fromBackup)
    {
        foreach (string path in new[] { savePath, savePath + ".bak" })
        {
            fromBackup = path != savePath;

            SaveData data;
            try
            {
                data = SaveFile.Read(path);
            }
            catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException || e is ArgumentException)
            {
                Debug.LogWarning($"Can't read {path}: {e.Message}", this);
                continue;
            }

            if (data == null) continue;

            // A newer game's save is not broken: don't fall back to the older .bak (the next save
            // would then overwrite the newer progress). Stop, and refuse to save until restart.
            if (data.version > SaveData.CurrentVersion)
            {
                Debug.LogWarning($"{path} is from a newer version of the game (save version {data.version}); not loading or saving.", this);
                newerSaveFound = true;
                break;
            }

            // Older saves: renamed IDs and moved data are upgraded before anything checks them.
            SaveMigration.Upgrade(data);

            string error = Validate(data);
            if (error != null)
            {
                Debug.LogWarning($"Rejected {path}: {error}", this);
                continue;
            }

            return data;
        }

        fromBackup = false;
        return null;
    }

    // Everything is checked before anything is loaded. Saveables refuse only what they can't repair;
    // single bad entries (unknown items, out-of-range values) are fixed in their Load with a warning.
    private string Validate(SaveData data)
    {
        // Older versions only lack newer parts: v1 restoredBuildings (TownProgress reads "nothing restored"),
        // v2 equipment (Equipment keeps its starting gear), v3 the tutorial (TutorialProgress treats it as done),
        // v4 the scene and position (SceneArrival; Continue falls back to the tutorial's scene).
        if (data.version < 1 || data.version > SaveData.CurrentVersion) return $"save version {data.version} is not supported";

        foreach (ISaveable saveable in saveables)
        {
            string error = saveable.Validate(data);
            if (error != null) return error;
        }

        return null;
    }

    [ContextMenu("Delete Save File")]
    private void DeleteSave()
    {
        SaveFile.Delete(savePath);
        Debug.Log($"Deleted {savePath}", this);
    }
}
