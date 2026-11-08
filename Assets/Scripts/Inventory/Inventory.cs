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

    public InventorySlot GetSlot(int index) =>
        index >= 0 && index < slots.Length ? slots[index] : null;

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

    private void EnsureSlots()
    {
        if (slots == null || slots.Length != slotCount)
            Array.Resize(ref slots, slotCount);

        for (int i = 0; i < slots.Length; i++)
            slots[i] ??= new InventorySlot();
    }
}
