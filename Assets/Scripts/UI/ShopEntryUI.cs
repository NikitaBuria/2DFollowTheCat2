using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One item for sale in the merchant panel: icon, price, click to buy. Reports the click to its owner.
[RequireComponent(typeof(Button))]
public class ShopEntryUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI priceText;

    private Button button;
    private MerchantStoreUI owner;
    private ItemData item;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(HandleClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(HandleClick);
    }

    public void Setup(MerchantStoreUI newOwner, ItemData newItem)
    {
        owner = newOwner;
        item = newItem;
        icon.sprite = item.icon;
        priceText.SetText("{0}", item.buyPrice);
    }

    private void HandleClick()
    {
        if (owner != null) owner.Buy(item);
    }
}
