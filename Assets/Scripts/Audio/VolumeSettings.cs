using UnityEngine;
using UnityEngine.Audio;

// The player's Music and SFX volume (0..1). Loads them from PlayerPrefs, applies them to the
// GameMixer and saves them when they change. One per scene (Core and MainMenu), so the volume
// is right from the start even if the settings screen is never opened. No UI here.
public class VolumeSettings : MonoBehaviour
{
    // Exposed GameMixer parameters (decibels).
    private const string MusicVolumeParameter = "MusicVolume";
    private const string SfxVolumeParameter = "SFXVolume";

    // PlayerPrefs keys.
    private const string MusicVolumeKey = "Settings.MusicVolume";
    private const string SfxVolumeKey = "Settings.SfxVolume";

    private const float DefaultVolume = 1f;
    // Log10(0) is -infinity, so the lowest slider value maps to -80 dB (silent) instead.
    private const float MinimumLinearVolume = 0.0001f;
    private const float DecibelsPerLog10 = 20f;

    [SerializeField] private AudioMixer audioMixer;

    public float musicVolume { get; private set; } = DefaultVolume;
    public float sfxVolume { get; private set; } = DefaultVolume;

    private void Awake()
    {
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, DefaultVolume);
    }

    // Start, not Awake: Unity ignores AudioMixer.SetFloat during Awake.
    private void Start()
    {
        ApplyToMixer(MusicVolumeParameter, musicVolume);
        ApplyToMixer(SfxVolumeParameter, sfxVolume);
    }

    private void OnDisable()
    {
        PlayerPrefs.Save(); // write to disk when the scene changes or the game quits
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        ApplyToMixer(MusicVolumeParameter, musicVolume);
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        ApplyToMixer(SfxVolumeParameter, sfxVolume);
    }

    // Slider value (linear 0..1) to decibels, which is how the mixer measures loudness.
    private void ApplyToMixer(string parameter, float volume)
    {
        float safeVolume = Mathf.Max(MinimumLinearVolume, volume);
        audioMixer.SetFloat(parameter, Mathf.Log10(safeVolume) * DecibelsPerLog10);
    }
}
