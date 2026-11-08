using UnityEngine;

// Anything in the world that can be collected: coins, items, later potions or keys.
public interface IPickable
{
    // Returns true when everything was taken (the pickup can be removed),
    // false when nothing or only part was taken (e.g. the inventory is full).
    bool TryPickUp(GameObject collector);
}
