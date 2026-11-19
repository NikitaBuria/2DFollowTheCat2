using System;
using UnityEngine;

// A fixed number of slots holding stacks of items. Fires OnChanged after every change so the UI
// can redraw. Saved slot by slot; itemDatabase turns saved IDs back into items.
public class Inventory : MonoBehaviour, ISaveable
{
    [SerializeField, Min(1)] private int slotCount = 20;
    [SerializeField] private InventorySlot[] slots;
    [SerializeField] private ItemDatabase itemDatabase;

    public event Action OnChanged;
    public event Action<int> OnSelectedSlotChanged;
    // After a hotbar use succeeded (PlayerSoundController listens).
    public event Action<ItemData> OnItemUsed;
    // Something new went into the bag (pickups, purchases, dev tools).
    public event Action<ItemData, int> OnItemAdded;

    public int count => slots.Length;

    // The hotbar slot picked with keys 1-7. Player states decide when it may change.
    public int selectedSlot { get; private set; }

    private void Awake()
    {
        EnsureSlots();
    }

    public InventorySlot GetSlot(int index) => index >= 0 && index < slots.Length ? slots[index] : null;

    // Removes the whole stack in this slot (selling). Returns whether anything was removed.
    public bool DeleteSlot(int index)
    {
        InventorySlot slot = GetSlot(index);
        if (slot == null || slot.isEmpty) return false;

        slot.Clear();
        OnChanged?.Invoke();
        return true;
    }

    // Puts exactly this stack in the slot (Equipment swaps worn items in and out with it).
    public void SetSlot(int index, ItemData item, int amount)
    {
        InventorySlot slot = GetSlot(index);
        if (slot == null) return;

        slot.Set(item, amount);
        OnChanged?.Invoke();
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Length || index == selectedSlot) return;

        selectedSlot = index;
        OnSelectedSlotChanged?.Invoke(selectedSlot);
    }

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

        OnChanged?.Invoke();
        OnItemAdded?.Invoke(item, accepted);

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

    // Hotbar keys: uses the item in this slot and removes one when it was used.
    public bool UseItem(int index, GameObject user)
    {
        InventorySlot slot = GetSlot(index);

        if (slot == null || slot.item is not IUsable usable) return false;
        if (!usable.Use(user)) return false;

        ItemData used = slot.item; // read before the last one is removed
        slot.Set(slot.item, slot.amount - 1);
        OnChanged?.Invoke();
        OnItemUsed?.Invoke(used);
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

    public void Save(SaveData data)
    {
        data.inventory = new SlotSaveData[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            data.inventory[i] = new SlotSaveData
            {
                itemId = slots[i].isEmpty ? null : ItemDatabase.GetId(slots[i].item),
                amount = slots[i].isEmpty ? 0 : slots[i].amount
            };
        }
    }

    // Only a missing database or a missing inventory are refused; bad slots are repaired in Load.
    public string Validate(SaveData data)
    {
        if (itemDatabase == null) return "Inventory has no ItemDatabase assigned";
        if (data.inventory == null) return "no inventory saved";
        return null;
    }

    // Repairs while loading: unknown items (renamed or removed in a newer build) and broken slots
    // become empty, stacks are cut to maxStack, and a save with a different slot count keeps the
    // slots that still exist.
    public void Load(SaveData data)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            SlotSaveData saved = i < data.inventory.Length ? data.inventory[i] : null;
            if (saved == null || saved.amount <= 0)
            {
                slots[i].Set(null, 0);
                continue;
            }

            ItemData item = itemDatabase.Find(saved.itemId);
            if (item == null)
            {
                Debug.LogWarning($"Save repaired: unknown item '{saved.itemId}' in slot {i} removed.", this);
                slots[i].Set(null, 0);
                continue;
            }

            int amount = Mathf.Min(saved.amount, item.maxStack);
            if (amount != saved.amount) Debug.LogWarning($"Save repaired: {saved.amount}x {saved.itemId} cut to a stack of {amount}.", this);
            slots[i].Set(item, amount);
        }

        for (int i = slots.Length; i < data.inventory.Length; i++)
        {
            SlotSaveData extra = data.inventory[i];
            if (extra != null && extra.amount > 0)
                Debug.LogWarning($"Save repaired: {extra.amount}x {extra.itemId} in slot {i} dropped (only {slots.Length} slots).", this);
        }

        OnChanged?.Invoke();
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
