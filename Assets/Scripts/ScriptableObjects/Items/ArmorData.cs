using UnityEngine;

// Helmet, body armor or boots. Equipment adds up the worn pieces' defense and takes it off every hit.
[CreateAssetMenu(fileName = "NewArmor", menuName = "Scriptable Objects/Armor Data")]
public class ArmorData : EquipmentData
{
    [SerializeField] private EquipmentSlotType armorSlot = EquipmentSlotType.Armor;
    [field: SerializeField, Min(0)] public int defense { get; private set; } = 1;

    public override EquipmentSlotType slotType => armorSlot;

    // Armor can't go in the weapon slot.
    private void OnValidate()
    {
        if (armorSlot == EquipmentSlotType.Weapon)
        {
            Debug.LogWarning($"{name}: armor can't use the Weapon slot, set back to Armor.", this);
            armorSlot = EquipmentSlotType.Armor;
        }
    }
}
