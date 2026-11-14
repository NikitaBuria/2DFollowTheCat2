using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The settings screen: Music and SFX sliders plus Back. Shows the saved values when opened and
// passes slider changes to VolumeSettings. Who opens and closes it (pause menu, main menu)
// listens to OnBackPressed; this script never hides itself.
public class SettingsPanelUI : MonoBehaviour
{
    [SerializeField] private VolumeSettings volumeSettings;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button backButton;

    public event Action OnBackPressed;

    private void OnEnable()
    {
        // WithoutNotify: showing the saved value must not count as a change.
        musicSlider.SetValueWithoutNotify(volumeSettings.musicVolume);
        sfxSlider.SetValueWithoutNotify(volumeSettings.sfxVolume);

        musicSlider.onValueChanged.AddListener(volumeSettings.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(volumeSettings.SetSfxVolume);
        backButton.onClick.AddListener(Back);

        // Keyboard / controller: Left/Right changes the selected slider, Up/Down moves between rows.
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(musicSlider.gameObject);
    }

    private void OnDisable()
    {
        musicSlider.onValueChanged.RemoveListener(volumeSettings.SetMusicVolume);
        sfxSlider.onValueChanged.RemoveListener(volumeSettings.SetSfxVolume);
        backButton.onClick.RemoveListener(Back);
    }

    private void Back()
    {
        OnBackPressed?.Invoke();
    }
}
