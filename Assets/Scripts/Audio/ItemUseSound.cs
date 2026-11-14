using UnityEngine;

// Plays the used item's ItemData.useSound (drinking a potion). Listens to Inventory.OnItemUsed.
[RequireComponent(typeof(Inventory))]
public class ItemUseSound : SoundPlayer
{
    private Inventory inventory;

    protected override void Awake()
    {
        base.Awake();
        inventory = GetComponent<Inventory>();
    }

    private void OnEnable()
    {
        inventory.OnItemUsed += PlayUse;
    }

    private void OnDisable()
    {
        inventory.OnItemUsed -= PlayUse;
    }

    private void PlayUse(ItemData item) => Play(item.useSound);
}
