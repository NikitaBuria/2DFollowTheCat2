using TMPro;
using UnityEngine;

// A short message on the shop panel after every buy or sell ("Bought Wooden Club for 60 coins",
// "Not enough coins"). Only listens to Merchant.OnTrade; ShopSound plays the trade sounds.
// The shop pauses the game (timeScale 0), so the timer uses unscaled time.
public class ShopFeedbackUI : MonoBehaviour
{
    [SerializeField] private Merchant merchant;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField, Min(0.1f)] private float showSeconds = 2f;
    [SerializeField] private Color successColor = new Color(0.95f, 0.8f, 0.45f);
    [SerializeField] private Color failColor = new Color(0.95f, 0.4f, 0.35f);

    [Header("Messages ({0} = item, {1} = coins)")]
    [SerializeField] private string boughtMessage = "Bought {0} for {1} coins";
    [SerializeField] private string soldMessage = "Sold {0} for {1} coins";
    [SerializeField] private string notEnoughCoinsMessage = "Not enough coins (need {1})";
    [SerializeField] private string inventoryFullMessage = "Inventory full";
    [SerializeField] private string cannotSellMessage = "The merchant won't buy the {0}";
    [SerializeField] private string notForSaleMessage = "Not for sale";

    private float hideAt;

    private void OnEnable()
    {
        messageText.gameObject.SetActive(false);
        if (merchant != null) merchant.OnTrade += HandleTrade;
    }

    private void OnDisable()
    {
        if (merchant != null) merchant.OnTrade -= HandleTrade;
    }

    private void Update()
    {
        if (messageText.gameObject.activeSelf && Time.unscaledTime >= hideAt)
            messageText.gameObject.SetActive(false);
    }

    private void HandleTrade(TradeResult result, ItemData item, int coins)
    {
        bool success = result == TradeResult.Bought || result == TradeResult.Sold;
        string itemName = item != null ? item.displayName : "";

        messageText.text = string.Format(GetMessage(result), itemName, coins);
        messageText.color = success ? successColor : failColor;
        messageText.gameObject.SetActive(true);
        hideAt = Time.unscaledTime + showSeconds;
    }

    private string GetMessage(TradeResult result) => result switch
    {
        TradeResult.Bought => boughtMessage,
        TradeResult.Sold => soldMessage,
        TradeResult.NotEnoughCoins => notEnoughCoinsMessage,
        TradeResult.InventoryFull => inventoryFullMessage,
        TradeResult.CannotSell => cannotSellMessage,
        TradeResult.NotForSale => notForSaleMessage,
        _ => throw new System.ArgumentOutOfRangeException(nameof(result), result, null),
    };
}
