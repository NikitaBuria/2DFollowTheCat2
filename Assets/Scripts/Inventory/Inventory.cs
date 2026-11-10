using System;
using UnityEngine;

// A fixed number of slots holding stacks of items. Fires OnChanged after every change so the UI
// (later) can redraw. Moving and splitting stacks come with the inventory UI.
public class Inventory : MonoBehaviour
{
    [SerializeField, Min(1)] private int slotCount = 20;
    [SerializeField] private InventorySlot[] slots;

    public event Action OnChanged;

    public int count => slots.Length;

    private void Awake()
    {
        EnsureSlots();
    }

    public InventorySlot GetSlot(int index) => index >= 0 && index < slots.Length ? slots[index] : null;

    // Returns how many were actually added (0 when the inventory is full).
    public int AddItemAndReturnAccepted(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return 0;

        int remaining = amount;

        // First top up stacks of the same item...
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (!slots[i].isEmpty && slots[i].item == item)
                remaining = slots[i].Add(item, remaining);
        }

        // ...then start new stacks in empty slots.
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (slots[i].isEmpty)
                remaining = slots[i].Add(item, remaining);
        }

        int accepted = amount - remaining;
        if (accepted == 0) return 0;

        Debug.Log($"{name} picked up {accepted}x {item.displayName}.", this);
        OnChanged?.Invoke();
        return accepted;
    }

    public bool HasItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        int found = 0;
        foreach (InventorySlot slot in slots)
        {
            if (!slot.isEmpty && slot.item == item)
                found += slot.amount;
        }

        return found >= amount;
    }

    // Removes the amount across stacks (e.g. when selling). Removes nothing if there isn't enough.
    public bool ConsumeItem(ItemData item, int amount = 1)
    {
        if (!HasItem(item, amount)) return false;

        for (int i = 0; i < slots.Length && amount > 0; i++)
        {
            if (slots[i].isEmpty || slots[i].item != item) continue;

            int taken = Mathf.Min(amount, slots[i].amount);
            slots[i].Set(item, slots[i].amount - taken);
            amount -= taken;
        }

        OnChanged?.Invoke();
        return true;
    }

    // Drag and drop between slots: moves into an empty slot, merges the same item up to maxStack
    // (the rest stays behind), swaps different items.
    public bool MoveOrMerge(int fromIndex, int toIndex)
    {
        if (!IsValidPair(fromIndex, toIndex)) return false;

        InventorySlot from = slots[fromIndex];
        InventorySlot to = slots[toIndex];

        if (to.isEmpty)
        {
            to.Set(from.item, from.amount);
            from.Clear();
        }
        else if (to.item == from.item)
        {
            int moved = Mathf.Min(to.item.maxStack - to.amount, from.amount);
            if (moved <= 0) return false;

            to.Set(to.item, to.amount + moved);
            from.Set(from.item, from.amount - moved);
        }
        else
        {
            ItemData item = from.item;
            int amount = from.amount;
            from.Set(to.item, to.amount);
            to.Set(item, amount);
        }

        OnChanged?.Invoke();
        return true;
    }

    // Right-drag: moves part of a stack into an empty slot or onto the same item. Never swaps;
    // refuses when it doesn't fit, so nothing moves.
    public bool Split(int fromIndex, int toIndex, int amount)
    {
        if (!IsValidPair(fromIndex, toIndex)) return false;

        InventorySlot from = slots[fromIndex];
        InventorySlot to = slots[toIndex];
        if (amount <= 0 || amount >= from.amount) return false;
        if (!to.isEmpty && to.item != from.item) return false;
        if (to.amount + amount > from.item.maxStack) return false;

        to.Set(from.item, to.amount + amount);
        from.Set(from.item, from.amount - amount);
        OnChanged?.Invoke();
        return true;
    }

    private bool IsValidPair(int fromIndex, int toIndex) =>
        fromIndex >= 0 && fromIndex < slots.Length &&
        toIndex >= 0 && toIndex < slots.Length &&
        fromIndex != toIndex && !slots[fromIndex].isEmpty;

    private void EnsureSlots()
    {
        if (slots == null || slots.Length != slotCount)
            Array.Resize(ref slots, slotCount);

        for (int i = 0; i < slots.Length; i++)
            slots[i] ??= new InventorySlot();
    }
}
