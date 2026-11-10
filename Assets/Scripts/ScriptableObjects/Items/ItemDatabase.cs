using System.Collections.Generic;
using UnityEngine;

// Every item that can be saved. Turns the ID in the save file back into the ItemData asset.
// The ID is the asset name, so renaming an item asset breaks old saves that hold it.
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> items = new();

    public static string GetId(ItemData item) => item != null ? item.name : null;

    // Returns null for unknown IDs.
    public ItemData Find(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;

        foreach (ItemData item in items)
        {
            if (item != null && item.name == itemId) return item;
        }

        return null;
    }
}
