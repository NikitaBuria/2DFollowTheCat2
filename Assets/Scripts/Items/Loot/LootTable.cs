using System.Collections.Generic;
using UnityEngine;

// What an enemy (later a chest or the boss) can drop. Shared asset, so several enemies can use one table.
[CreateAssetMenu(fileName = "NewLootTable", menuName = "Scriptable Objects/Loot Table")]
public class LootTable : ScriptableObject
{
    [SerializeField] private List<LootDrop> drops = new List<LootDrop>();

    // Pickups land at random points in this circle around the drop position (top-down, no gravity).
    [SerializeField, Min(0f)] private float dropRadius = 0.5f;

    public void DropLoot(Vector2 position)
    {
        foreach (LootDrop drop in drops)
        {
            if (drop == null || !drop.ShouldDrop()) continue;

            int amount = drop.GetRandomAmount();
            for (int i = 0; i < amount; i++)
            {
                Vector2 spawnPosition = position + Random.insideUnitCircle * dropRadius;
                GameObject spawned = Instantiate(drop.prefab, spawnPosition, Quaternion.identity);

                if (drop.item != null && spawned.TryGetComponent(out ItemPickup itemPickup))
                    itemPickup.Configure(drop.item, 1);
            }
        }
    }
}
