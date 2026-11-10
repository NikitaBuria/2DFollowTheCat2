using UnityEngine;

// An item that does something when used from the hotbar (keys 1-7).
// Using is the item's job; removing one from the stack is the Inventory's job.
public interface IUsable
{
    // Returns true when the item was used, so the inventory removes one
    // (e.g. a health potion at full health returns false and is kept).
    bool Use(GameObject user);
}
