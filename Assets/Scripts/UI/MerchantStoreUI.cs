using System.Collections.Generic;
using UnityEngine;

// Builds one ShopEntryUI per item in the merchant's ShopStock and forwards clicks to the Merchant.
// Visibility is UIManager's job. Rebuilds every time the shop opens, because the stock grows
// with the town tier.
public class MerchantStoreUI : MonoBehaviour
{
    [SerializeField] private Merchant merchant;
    [SerializeField] private ShopEntryUI entryPrefab;
    [SerializeField] private Transform entriesParent;

    private readonly List<ShopEntryUI> entries = new();

    private void OnEnable()
    {
        // Only the entries built here: the panel can hold Astra's decoration too.
        foreach (ShopEntryUI oldEntry in entries)
            Destroy(oldEntry.gameObject);
        entries.Clear();

        foreach (ItemData item in merchant.stock.itemList)
        {
            if (item == null) continue;

            ShopEntryUI entry = Instantiate(entryPrefab, entriesParent);
            entry.Setup(this, item);
            entries.Add(entry);
        }
    }

    public void Buy(ItemData item)
    {
        // Later: feedback when it fails (not enough coins, inventory full).
        merchant.TryBuy(item);
    }
}
