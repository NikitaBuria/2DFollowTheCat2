using System.Collections.Generic;
using UnityEngine;

// Upgrades the data of an older save before it is validated and loaded, one version step at a time.
// Only for changes the ISaveables can't handle on their own (renamed IDs, moved data); a part that is
// simply missing in an old save is still handled by its ISaveable, which reads data.version for that.
// So data.version is never changed here: it keeps saying which parts the file really has.
public static class SaveMigration
{
    // v6: item assets renamed (the asset name is the save ID).
    private static readonly Dictionary<string, string> RenamedItemIds = new()
    {
        { "WoodenCLub", "WoodenClub" },
        { "LanterDefault", "Lantern" },
    };

    // v6: coins stopped being an inventory item; old "Coins" stacks go into the wallet.
    private const string OldCoinsItemId = "Coins";

    public static void Upgrade(SaveData data)
    {
        if (data.version < 6) UpgradeToVersion6(data);
    }

    private static void UpgradeToVersion6(SaveData data)
    {
        if (data.inventory != null)
        {
            foreach (SlotSaveData slot in data.inventory)
            {
                if (slot == null || string.IsNullOrEmpty(slot.itemId)) continue;

                if (slot.itemId == OldCoinsItemId)
                {
                    data.coins += Mathf.Max(0, slot.amount);
                    slot.itemId = "";
                    slot.amount = 0;
                }
                else
                {
                    slot.itemId = Rename(slot.itemId);
                }
            }
        }

        if (data.equipment != null)
        {
            for (int i = 0; i < data.equipment.Length; i++)
                data.equipment[i] = Rename(data.equipment[i]);
        }
    }

    private static string Rename(string itemId) =>
        itemId != null && RenamedItemIds.TryGetValue(itemId, out string newId) ? newId : itemId;
}
