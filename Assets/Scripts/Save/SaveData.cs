using System;
using System.Collections.Generic;

// Everything one save file holds. Plain data only, written to JSON by SaveFile.
// Each ISaveable fills and reads its own part. Bump Version when the layout changes;
// SaveManager accepts older versions only when every ISaveable can read them (v1 had no town).
[Serializable]
public class SaveData
{
    public const int CurrentVersion = 2;

    public int version = CurrentVersion;
    public int health;
    public int coins;
    public SlotSaveData[] inventory;
    public List<string> restoredBuildings; // building IDs (asset names), added in version 2
}

// One inventory slot. Items are saved by ID (the ItemData asset name), because a JSON file
// can't hold a reference to an asset; ItemDatabase turns the ID back into the asset on load.
[Serializable]
public class SlotSaveData
{
    public string itemId;
    public int amount;
}
