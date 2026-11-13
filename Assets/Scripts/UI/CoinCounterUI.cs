using TMPro;
using UnityEngine;

// Shows the player's coins. Only listens to Wallet.OnMoneyChanged; refreshes in OnEnable
// because the panel it sits in is switched off most of the time.
public class CoinCounterUI : MonoBehaviour
{
    [SerializeField] private Wallet wallet;
    [SerializeField] private TextMeshProUGUI coinTextAmount;

    private void OnEnable()
    {
        wallet.OnMoneyChanged += UpdateCoinsUI;
        UpdateCoinsUI(wallet.coins);
    }

    private void OnDisable()
    {
        wallet.OnMoneyChanged -= UpdateCoinsUI;
    }

    private void UpdateCoinsUI(int coinAmount)
    {
        coinTextAmount.SetText("{0}", coinAmount);
    }
}
