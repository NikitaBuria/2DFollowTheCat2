using UnityEngine;

// An item lying in the world. Goes into the collector's Inventory; stays if it doesn't fit.
public class ItemPickup : Pickup
{
    [SerializeField] private ItemData item;
    [SerializeField, Min(1)] private int amount = 1;

    [SerializeField] private SpriteRenderer spriteRenderer;

    // Called by LootTable after spawning, so one prefab can show any item.
    public void Configure(ItemData newItem, int newAmount)
    {
        item = newItem;
        amount = Mathf.Max(1, newAmount);

        if (spriteRenderer != null && item != null && item.icon != null)
            spriteRenderer.sprite = item.icon;
    }

    public override bool TryPickUp(GameObject collector)
    {
        if (item == null) return false;
        if (!collector.TryGetComponent(out Inventory inventory)) return false;

        amount -= inventory.AddItemAndReturnAccepted(item, amount);
        return amount <= 0;
    }
}
