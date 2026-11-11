using UnityEngine;

// Read-only description of one kind of item (loot, later weapons and armor inherit from it).
// The inventory stores references to these assets plus an amount; never change values at runtime.
[CreateAssetMenu(fileName = "NewItemData", menuName = "Scriptable Objects/Item Data")]
public class ItemData : ScriptableObject
{
    [field: SerializeField] public string itemName { get; private set; } = "Item";
    [field: SerializeField] public Sprite icon { get; private set; }
    [field: SerializeField, Min(1)] public int maxStack { get; private set; } = 10;

    // Coins the merchant pays for one of these.
    [field: SerializeField, Min(0)] public int sellPrice { get; private set; } = 1;

    // Coins the merchant asks for one of these (only matters for items in a ShopStock).
    [field: SerializeField, Min(0)] public int buyPrice { get; private set; } = 5;

    // Falls back to the asset name so an empty field still shows something useful.
    public string displayName => string.IsNullOrWhiteSpace(itemName) ? name : itemName;
}
