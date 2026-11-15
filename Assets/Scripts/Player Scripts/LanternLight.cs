using UnityEngine;
using UnityEngine.Rendering.Universal;

// The light around the player (on the Player's LanternLight child, a Point Light 2D).
// Shines with whatever light the held weapon gives (EquipmentData.lightRadius / lightColor /
// lightIntensity), off when it gives none. Only listens to Equipment, like the other views.
// A small flicker makes it feel like a flame. Darkness itself comes from each scene's Global Light 2D.
[RequireComponent(typeof(Light2D))]
public class LanternLight : MonoBehaviour
{
    [SerializeField] private Equipment equipment;

    [Header("Flicker")]
    [Tooltip("How much the brightness wobbles (0 = steady).")]
    [SerializeField, Range(0f, 0.5f)] private float flickerAmount = 0.08f;
    [SerializeField, Min(0f)] private float flickerSpeed = 3f;

    // The light grows to its full radius from this much of it, so it doesn't pop on.
    private const float InnerRadiusShare = 0.35f;

    // Below this the lantern would be invisible anyway (daylight), so it stays off.
    private const float MinVisibleIntensity = 0.05f;

    private Light2D light2D;
    private float baseIntensity;
    private float noiseSeed;
    // 1 - the scene's Global Light 2D intensity: full in darkness, nothing in daylight.
    private float darkness = 1f;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
        noiseSeed = Random.value * 100f;
        darkness = Mathf.Clamp01(1f - FindGlobalLightIntensity());
    }

    // The scene's Global Light 2D sets how dark it is; no global light = treat as dark.
    private float FindGlobalLightIntensity()
    {
        foreach (Light2D light in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
        {
            if (light.lightType == Light2D.LightType.Global && light.gameObject.scene == gameObject.scene)
                return light.intensity;
        }

        return 0f;
    }

    private void OnEnable()
    {
        equipment.OnEquipmentChanged += HandleEquipmentChanged;
    }

    private void OnDisable()
    {
        equipment.OnEquipmentChanged -= HandleEquipmentChanged;
    }

    // Start: Equipment's Awake (starting gear) has run by now.
    private void Start()
    {
        Show(equipment.GetEquipped(EquipmentSlotType.Weapon));
    }

    private void HandleEquipmentChanged(EquipmentSlotType slotType, EquipmentData item)
    {
        if (slotType == EquipmentSlotType.Weapon) Show(item);
    }

    private void Show(EquipmentData item)
    {
        // Scaled by the scene's darkness, so a lantern doesn't blow out a daylight scene.
        baseIntensity = item != null && item.givesLight ? item.lightIntensity * darkness : 0f;
        bool lit = baseIntensity >= MinVisibleIntensity;
        light2D.enabled = lit;
        if (!lit) return;

        light2D.pointLightOuterRadius = item.lightRadius;
        light2D.pointLightInnerRadius = item.lightRadius * InnerRadiusShare;
        light2D.color = item.lightColor;
        light2D.intensity = baseIntensity;
    }

    private void Update()
    {
        if (!light2D.enabled || flickerAmount <= 0f) return;

        // Perlin noise: smooth random wobble around the base brightness.
        float noise = Mathf.PerlinNoise(noiseSeed, Time.time * flickerSpeed) * 2f - 1f;
        light2D.intensity = baseIntensity * (1f + noise * flickerAmount);
    }
}
