using System;
using UnityEngine;

// One stack in the inventory: an item and how many. Plain class, owned by Inventory.
[Serializable]
public class InventorySlot
{
    [field: SerializeField] public ItemData item { get; private set; }
    [field: SerializeField, Min(0)] public int amount { get; private set; }

    public bool isEmpty => item == null || amount <= 0;

    // Adds as much as fits in this stack; returns what is left over.
    public int Add(ItemData newItem, int requested)
    {
        if (newItem == null || requested <= 0 || (!isEmpty && item != newItem))
            return requested;

        if (isEmpty)
        {
            item = newItem;
            amount = 0;
        }

        int added = Mathf.Min(requested, newItem.maxStack - amount);
        amount += added;
        return requested - added;
    }

    public void Set(ItemData newItem, int newAmount)
    {
        item = newAmount > 0 ? newItem : null;
        amount = Mathf.Max(0, newAmount);
    }

    public void Clear() => Set(null, 0);
}
