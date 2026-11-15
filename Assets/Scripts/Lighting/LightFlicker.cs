using UnityEngine;
using UnityEngine.Rendering.Universal;

// Makes a Light 2D feel alive: its brightness wobbles smoothly around the value set in the Inspector.
// Fast and small for candles and torches, slow and deep for a pulsing glow (the abyss, the portal).
[RequireComponent(typeof(Light2D))]
public class LightFlicker : MonoBehaviour
{
    [Tooltip("How much the brightness wobbles (0 = steady, 0.2 = up to 20% brighter or darker).")]
    [SerializeField, Range(0f, 1f)] private float flickerAmount = 0.15f;
    [Tooltip("How fast it wobbles. Candles about 4, a slow pulse about 0.5.")]
    [SerializeField, Min(0f)] private float flickerSpeed = 4f;

    // Random.value is 0..1; spread the seeds so no two lights wobble in step.
    private const float SeedRange = 100f;

    private Light2D light2D;
    private float baseIntensity;
    private float noiseSeed;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
        baseIntensity = light2D.intensity;
        noiseSeed = Random.value * SeedRange;
    }

    private void Update()
    {
        // Perlin noise: smooth random values, so the light drifts instead of jumping.
        float noise = Mathf.PerlinNoise(noiseSeed, Time.time * flickerSpeed) * 2f - 1f;
        light2D.intensity = baseIntensity * (1f + noise * flickerAmount);
    }
}
