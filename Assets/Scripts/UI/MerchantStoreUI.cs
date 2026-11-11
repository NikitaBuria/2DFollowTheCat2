using UnityEngine;

// Builds one ShopEntryUI per item in the merchant's ShopStock and forwards clicks to the Merchant.
// Visibility is UIManager's job. Like InventoryUI, builds in Start (after every Awake).
public class MerchantStoreUI : MonoBehaviour
{
    [SerializeField] private Merchant merchant;
    [SerializeField] private ShopEntryUI entryPrefab;
    [SerializeField] private Transform entriesParent;

    private void Start()
    {
        foreach (ItemData item in merchant.stock.itemList)
        {
            if (item == null) continue;

            ShopEntryUI entry = Instantiate(entryPrefab, entriesParent);
            entry.Setup(this, item);
        }
    }

    public void Buy(ItemData item)
    {
        // Later: feedback when it fails (not enough coins, inventory full).
        merchant.TryBuy(item);
    }
}
