using UnityEngine;

// Plays a sound after every buy or sell attempt. Listens to Merchant.OnTrade, like ShopFeedbackUI
// does for the text, so neither the merchant nor the feedback text knows about audio.
// Plays on the always-active UIClickAudio source: the shop panel may close while the sound plays.
public class ShopSound : MonoBehaviour
{
    // Scene reference (the merchant lives in Town), set as an override on Town's Core instance.
    [SerializeField] private Merchant merchant;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SoundData boughtSound;
    [SerializeField] private SoundData soldSound;
    [SerializeField] private SoundData failSound;

    private void OnEnable()
    {
        if (merchant != null) merchant.OnTrade += PlayTradeSound;
    }

    private void OnDisable()
    {
        if (merchant != null) merchant.OnTrade -= PlayTradeSound;
    }

    private void PlayTradeSound(TradeResult result, ItemData item, int coins)
    {
        SoundData sound = result switch
        {
            TradeResult.Bought => boughtSound,
            TradeResult.Sold => soldSound,
            _ => failSound,
        };
        if (sound != null) sound.PlayOn(audioSource);
    }
}
