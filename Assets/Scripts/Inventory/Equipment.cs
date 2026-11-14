using System;
using UnityEngine;

// What the player wears: one EquipmentData per EquipmentSlotType. Equipping swaps with the
// inventory slot the item came from, so it never needs free space. Fires OnEquipmentChanged;
// Player passes the weapon to PlayerCombat, the UI listens too. Saved by item ID like Inventory.
// As the player's IDamageModifier, worn armor's defense lowers every hit (Health asks).
[RequireComponent(typeof(Inventory))]
public class Equipment : MonoBehaviour, ISaveable, IDamageModifier
{
    [SerializeField] private WeaponData startingWeapon;
    [SerializeField] private ItemDatabase itemDatabase;

    // Armor never blocks a hit completely.
    private const int MinDamagePerHit = 1;

    private static readonly int SlotCount = Enum.GetValues(typeof(EquipmentSlotType)).Length;

    private readonly EquipmentData[] equipped = new EquipmentData[SlotCount];
    private Inventory inventory;

    public event Action<EquipmentSlotType, EquipmentData> OnEquipmentChanged;
    // Only when the player puts an item on (TryEquip), never on Load (EquipSound listens).
    public event Action<EquipmentData> OnItemEquipped;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
        equipped[(int)EquipmentSlotType.Weapon] = startingWeapon;
    }

    public EquipmentData GetEquipped(EquipmentSlotType slotType) => equipped[(int)slotType];

    // Sum of every worn ArmorData's defense.
    public int totalDefense
    {
        get
        {
            int defense = 0;
            foreach (EquipmentData item in equipped)
            {
                if (item is ArmorData armor)
                    defense += armor.defense;
            }
            return defense;
        }
    }

    public int ModifyIncomingDamage(int amount) =>
        amount <= 0 ? 0 : Mathf.Max(MinDamagePerHit, amount - totalDefense);

    // Wears the item in this inventory slot; whatever was worn goes into that same slot.
    public bool TryEquip(int inventoryIndex)
    {
        InventorySlot slot = inventory.GetSlot(inventoryIndex);
        if (slot == null || slot.item is not EquipmentData item || slot.amount != 1) return false;

        EquipmentData previous = GetEquipped(item.slotType);
        inventory.SetSlot(inventoryIndex, previous, previous != null ? 1 : 0);
        SetEquipped(item.slotType, item);
        OnItemEquipped?.Invoke(item);
        return true;
    }

    // Drag from an equipment box onto an inventory slot: an empty slot takes it, a slot holding
    // equipment for the same box swaps; anything else is refused.
    public bool UnequipTo(EquipmentSlotType slotType, int inventoryIndex)
    {
        EquipmentData item = GetEquipped(slotType);
        InventorySlot target = inventory.GetSlot(inventoryIndex);
        if (item == null || target == null) return false;

        if (target.item is EquipmentData other && other.slotType == slotType)
            return TryEquip(inventoryIndex);

        if (!target.isEmpty) return false;

        inventory.SetSlot(inventoryIndex, item, 1);
        SetEquipped(slotType, null);
        return true;
    }

    public void Save(SaveData data)
    {
        data.equipment = new string[SlotCount];
        for (int i = 0; i < SlotCount; i++)
            data.equipment[i] = ItemDatabase.GetId(equipped[i]);
    }

    public string Validate(SaveData data)
    {
        if (data.equipment == null) return null; // saves before version 3: keep the starting gear
        if (itemDatabase == null) return "Equipment has no ItemDatabase assigned";
        if (data.equipment.Length != SlotCount) return $"expected {SlotCount} equipment slots";

        for (int i = 0; i < SlotCount; i++)
        {
            string id = data.equipment[i];
            if (string.IsNullOrEmpty(id)) continue;

            if (itemDatabase.Find(id) is not EquipmentData item) return $"'{id}' is not equipment (missing from the ItemDatabase?)";
            if ((int)item.slotType != i) return $"'{id}' can't be worn in the {(EquipmentSlotType)i} slot";
        }

        return null;
    }

    public void Load(SaveData data)
    {
        if (data.equipment == null) return;

        for (int i = 0; i < SlotCount; i++)
            SetEquipped((EquipmentSlotType)i, itemDatabase.Find(data.equipment[i]) as EquipmentData);
    }

    private void SetEquipped(EquipmentSlotType slotType, EquipmentData item)
    {
        equipped[(int)slotType] = item;
        OnEquipmentChanged?.Invoke(slotType, item);
    }
}
