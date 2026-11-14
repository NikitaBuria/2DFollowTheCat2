using UnityEngine;

// Plays the equipped item's EquipmentData.equipSound. Listens to Equipment.OnItemEquipped, which only
// fires for the player's own equips (not when a save is loaded).
[RequireComponent(typeof(Equipment))]
public class EquipSound : SoundPlayer
{
    private Equipment equipment;

    protected override void Awake()
    {
        base.Awake();
        equipment = GetComponent<Equipment>();
    }

    private void OnEnable()
    {
        equipment.OnItemEquipped += PlayEquip;
    }

    private void OnDisable()
    {
        equipment.OnItemEquipped -= PlayEquip;
    }

    private void PlayEquip(EquipmentData item) => Play(item.equipSound);
}
