using UnityEngine;
using UnityEngine.UI;

// Shows the player's health as a filled Image. Only listens to Health.OnHealthChanged.
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image fillImage;

    private void OnEnable()
    {
        health.OnHealthChanged += Show;
        Show(health.currentHealth, health.maxHealth);
    }

    private void OnDisable()
    {
        health.OnHealthChanged -= Show;
    }

    private void Show(int current, int max)
    {
        // max is 0 until the Player's Awake calls Initialize, which fires the event again.
        if (max <= 0) return;

        fillImage.fillAmount = (float)current / max;
    }
}
