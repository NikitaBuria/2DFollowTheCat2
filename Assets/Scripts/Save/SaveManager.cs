using System;
using System.IO;
using UnityEngine;

// Saves and loads the player's ISaveable components (Health, Wallet, Inventory) and the ones on
// this object (TownProgress) through SaveFile.
// Loading validates everything first and falls back to the .bak file, so a broken save never half-loads.
// Ported from MetrooDvania's checkpoint save, without the respawn, loot and debug parts.
public class SaveManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private string saveFileName = "save.json";
    [Tooltip("Load the save file when the scene starts. Turn off to test from a fresh start.")]
    [SerializeField] private bool loadOnStart = true;

    private ISaveable[] saveables;
    private string savePath => SaveFile.GetPath(saveFileName);

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

    // Debug keys (F5/F9) through PlayerInputReader. Later the game saves on its own (town, checkpoints).
    private void Update()
    {
        if (player.playerInputReader.savePressedThisFrame) Save();
        if (player.playerInputReader.loadPressedThisFrame) Load();
    }

    public bool Save()
    {
        // Never save a dead player: the save would be useless and Validate would reject it.
        if (!player.playerHealth.isAlive) return false;

        SaveData data = new();
        foreach (ISaveable saveable in saveables)
            saveable.Save(data);

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

        foreach (string path in new[] { savePath, savePath + ".bak" })
        {
            SaveData data;
            try
            {
                data = SaveFile.Read(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            {
                Debug.LogWarning($"Can't read {path}: {e.Message}", this);
                continue;
            }

            if (data == null) continue;

            string error = Validate(data);
            if (error != null)
            {
                Debug.LogWarning($"Rejected {path}: {error}", this);
                continue;
            }

            foreach (ISaveable saveable in saveables)
                saveable.Load(data);

            Debug.Log(path == savePath ? "Game loaded." : "Game loaded from the backup save.", this);
            return true;
        }

        return false;
    }

    // Everything is checked before anything is loaded.
    private string Validate(SaveData data)
    {
        // Version 1 only lacks restoredBuildings, which TownProgress reads as "nothing restored".
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
