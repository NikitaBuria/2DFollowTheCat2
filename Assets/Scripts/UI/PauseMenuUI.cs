using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The pause menu's buttons. UIManager shows it in GameState.Paused; Escape still toggles it
// through GameStateManager. Settings and Controls (the ? button) are sub-screens inside the pause
// menu (not game states): this script swaps between the button panel and those panels.
public class PauseMenuUI : MonoBehaviour
{
    private const string MainMenuScene = "MainMenu";

    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private GameObject buttonsPanel;
    [SerializeField] private SettingsPanelUI settingsPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button helpButton;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private Button controlsBackButton;

    private void OnEnable()
    {
        resumeButton.onClick.AddListener(Resume);
        settingsButton.onClick.AddListener(OpenSettings);
        exitButton.onClick.AddListener(Exit);
        helpButton.onClick.AddListener(OpenControls);
        controlsBackButton.onClick.AddListener(CloseControls);
        settingsPanel.OnBackPressed += CloseSettings;

        // Every pause starts on the buttons, even if it was closed (Escape) while in Settings or Controls.
        ShowSettings(false);
        Select(resumeButton);
    }

    private void OnDisable()
    {
        resumeButton.onClick.RemoveListener(Resume);
        settingsButton.onClick.RemoveListener(OpenSettings);
        exitButton.onClick.RemoveListener(Exit);
        helpButton.onClick.RemoveListener(OpenControls);
        controlsBackButton.onClick.RemoveListener(CloseControls);
        settingsPanel.OnBackPressed -= CloseSettings;
    }

    private void Resume()
    {
        gameStateManager.ChangeState(GameState.Gameplay);
    }

    private void OpenSettings()
    {
        ShowSettings(true); // SettingsPanelUI selects its first slider
    }

    private void CloseSettings()
    {
        ShowSettings(false);
        Select(settingsButton);
    }

    private void OpenControls()
    {
        buttonsPanel.SetActive(false);
        controlsPanel.SetActive(true);
        Select(controlsBackButton);
    }

    private void CloseControls()
    {
        ShowSettings(false);
        Select(helpButton);
    }

    // Saves like a door does, so quitting to the menu keeps coins, items and town progress.
    // SaveManager refuses while the player is dead; the death respawn saves anyway, so leave regardless.
    // Time runs again before the menu loads.
    private void Exit()
    {
        saveManager.Save();
        gameStateManager.ChangeState(GameState.Gameplay);
        SceneLoader.Load(MainMenuScene, null);
    }

    private void ShowSettings(bool show)
    {
        buttonsPanel.SetActive(!show);
        settingsPanel.gameObject.SetActive(show);
        controlsPanel.SetActive(false);
    }

    // Keyboard / controller can press it right away.
    private static void Select(Selectable selectable)
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(selectable.gameObject);
    }
}
