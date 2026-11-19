using UnityEngine;
using UnityEngine.UI;

// Shows the player's health as a filled Image plus a cat badge on the frame (CatHealthBadge shader)
// whose liquid settles over liquidSettleTime and whose cat pose shows how hurt the player is.
// Only listens to Health.OnHealthChanged. With a normal material on the frame it is a plain filled bar.
[RequireComponent(typeof(Image))]
public class HealthBarUI : MonoBehaviour
{
    private static readonly int WaterLevel = Shader.PropertyToID("_WaterLevel");
    private static readonly int Fatigue = Shader.PropertyToID("_Fatigue");

    // Fatigue phases in the shader: 0 run, 1 walk, 2 seated crest, 3 prone, 4 belly-up (dead).
    private const int DeadPhase = 4;

    [SerializeField] private Health health;
    [SerializeField] private Image fillImage;
    [SerializeField, Min(0.01f)] private float liquidSettleTime = 0.85f;
    [Tooltip("Health fraction at or below which the cat walks, sits and lies down.")]
    [SerializeField, Range(0f, 1f)] private float walkBelow = 0.8f;
    [SerializeField, Range(0f, 1f)] private float sitBelow = 0.5f;
    [SerializeField, Range(0f, 1f)] private float proneBelow = 0.2f;

    private Image frame;
    private Material originalMaterial;
    private Material badge;
    private float startLevel;
    private float shownLevel;
    private float targetLevel;
    private float elapsed;
    private bool initialized;

    private void Awake()
    {
        frame = GetComponent<Image>();
        originalMaterial = frame.material;
        if (!originalMaterial.HasProperty(WaterLevel)) return;

        badge = new Material(originalMaterial);
        frame.material = badge;
    }

    private void OnEnable()
    {
        initialized = false;
        health.OnHealthChanged += Show;
        Show(health.currentHealth, health.maxHealth);
    }

    private void OnDisable()
    {
        health.OnHealthChanged -= Show;
    }

    private void OnDestroy()
    {
        if (badge == null) return;
        if (frame != null) frame.material = originalMaterial;
        Destroy(badge);
    }

    private void Show(int current, int max)
    {
        // max is 0 until the Player's Awake calls Initialize, which fires the event again.
        if (max <= 0) return;
        targetLevel = Mathf.Clamp01((float)current / max);
        if (badge != null) badge.SetFloat(Fatigue, GetFatiguePhase(targetLevel));

        // The first value snaps into place; later changes settle in Update.
        if (!initialized)
        {
            shownLevel = targetLevel;
            ApplyLevel();
            initialized = true;
        }

        startLevel = shownLevel;
        elapsed = 0f;
    }

    // The fill bar and the badge's liquid always show the same level.
    private void ApplyLevel()
    {
        fillImage.fillAmount = shownLevel;
        if (badge != null) badge.SetFloat(WaterLevel, shownLevel);
    }

    private int GetFatiguePhase(float level)
    {
        if (level <= 0f) return DeadPhase;
        if (level <= proneBelow) return 3;
        if (level <= sitBelow) return 2;
        if (level <= walkBelow) return 1;
        return 0;
    }

    private void Update()
    {
        if (shownLevel == targetLevel) return;
        // Unscaled: menus set timeScale to 0, and the bar should still settle behind them.
        elapsed += Time.unscaledDeltaTime;
        shownLevel = Mathf.SmoothStep(startLevel, targetLevel, elapsed / liquidSettleTime);
        if (elapsed >= liquidSettleTime) shownLevel = targetLevel;
        ApplyLevel();
    }
}
