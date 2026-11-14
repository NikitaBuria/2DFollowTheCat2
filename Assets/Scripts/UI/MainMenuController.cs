using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The main menu scene (Astra's MainMenu prefab). Play opens the first game scene, whose SaveManager
// loads the save if there is one, so Play works as both "New Game" and "Continue".
// Settings swaps the three buttons for the settings panel; its Back swaps them back.
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private SettingsPanelUI settingsPanel;
    [Tooltip("Scene name as listed in Build Profiles > Scene List.")]
    [SerializeField] private string firstScene = "Town";

    private void OnEnable()
    {
        playButton.onClick.AddListener(Play);
        settingsButton.onClick.AddListener(OpenSettings);
        exitButton.onClick.AddListener(Exit);
        settingsPanel.OnBackPressed += CloseSettings;

        ShowSettings(false);
    }

    private void OnDisable()
    {
        playButton.onClick.RemoveListener(Play);
        settingsButton.onClick.RemoveListener(OpenSettings);
        exitButton.onClick.RemoveListener(Exit);
        settingsPanel.OnBackPressed -= CloseSettings;
    }

    private void Play()
    {
        // No spawn ID: the player starts at the scene's own PlayerSpawnPoint.
        SceneLoader.Load(firstScene, null);
    }

    private void OpenSettings()
    {
        ShowSettings(true); // SettingsPanelUI selects its first slider
    }

    private void CloseSettings()
    {
        ShowSettings(false);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
    }

    private void ShowSettings(bool show)
    {
        playButton.gameObject.SetActive(!show);
        settingsButton.gameObject.SetActive(!show);
        exitButton.gameObject.SetActive(!show);
        settingsPanel.gameObject.SetActive(show);
    }

    private void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
