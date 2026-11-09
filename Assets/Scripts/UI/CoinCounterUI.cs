using UnityEngine;
using TMPro;

public class CoinCounterUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Wallet wallet;
    [SerializeField] private TextMeshProUGUI coinTextAmount;




    private void OnEnable()
    {
         Subscribe();
         UpdateCoinsUI(wallet.coins);
        
    }
    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        wallet.OnMoneyChanged += UpdateCoinsUI;
        
    }   
    private void Unsubscribe ()
    {
                wallet.OnMoneyChanged -= UpdateCoinsUI;

    }



    private void UpdateCoinsUI(int coinAmount)
    {
        coinTextAmount.SetText("{0}", coinAmount);
    }


    
}
