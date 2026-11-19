using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

// The main menu scene (Astra's MainMenu prefab).
// New Game deletes the save (asks once first) and starts the intro in the opening cave.
// Continue loads the saved scene at the saved position (saves before v5: the current tutorial step's
// scene, or Town once the tutorial is done); that scene's SaveManager then loads everything else.
// Greyed out without a save.
// Settings swaps the buttons for the settings panel; its Back swaps them back.
public class MainMenuController : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("playButton")] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button helpButton;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private Button controlsBackButton;
    [SerializeField] private SettingsPanelUI settingsPanel;
    [SerializeField] private TextMeshProUGUI newGameLabel;
    [SerializeField] private string confirmNewGameText = "Start over?";

    [Header("Scenes (names as in Build Profiles > Scene List)")]
    [SerializeField] private string newGameScene = "OpeningCave";
    [SerializeField, FormerlySerializedAs("firstScene")] private string afterTutorialScene = "Town";
    [SerializeField] private TutorialSequence tutorial;

    private string newGameText;
    private bool confirmingNewGame;

    private string savePath => SaveFile.GetPath(SaveFile.DefaultFileName);

    private void Awake()
    {
        newGameText = newGameLabel.text;
    }

    private void OnEnable()
    {
        newGameButton.onClick.AddListener(NewGame);
        continueButton.onClick.AddListener(Continue);
        settingsButton.onClick.AddListener(OpenSettings);
        exitButton.onClick.AddListener(Exit);
        helpButton.onClick.AddListener(OpenControls);
        controlsBackButton.onClick.AddListener(CloseControls);
        settingsPanel.OnBackPressed += CloseSettings;

        continueButton.interactable = ReadSave() != null;
        ShowSettings(false);
    }

    private void OnDisable()
    {
        newGameButton.onClick.RemoveListener(NewGame);
        continueButton.onClick.RemoveListener(Continue);
        settingsButton.onClick.RemoveListener(OpenSettings);
        exitButton.onClick.RemoveListener(Exit);
        helpButton.onClick.RemoveListener(OpenControls);
        controlsBackButton.onClick.RemoveListener(CloseControls);
        settingsPanel.OnBackPressed -= CloseSettings;
    }

    // A save exists: the first click only asks, the second one starts over.
    private void NewGame()
    {
        if (ReadSave() != null && !confirmingNewGame)
        {
            confirmingNewGame = true;
            newGameLabel.text = confirmNewGameText;
            return;
        }

        SaveFile.Delete(savePath);
        SceneLoader.Load(newGameScene, null);
    }

    private void Continue()
    {
        SaveData data = ReadSave();
        if (data == null) return;

        // v5+: back to the exact scene and position of the save.
        if (data.version >= SceneArrival.FirstVersionWithLocation
            && !string.IsNullOrEmpty(data.sceneName)
            && Application.CanStreamedLevelBeLoaded(data.sceneName))
        {
            SceneLoader.Load(data.sceneName, data.position);
            return;
        }

        GuideStep step = data.version >= TutorialProgress.FirstVersionWithTutorial ? tutorial.GetStep(data.tutorialStep) : null;
        SceneLoader.Load(step != null ? step.sceneName : afterTutorialScene, null);
    }

    // The main save, or its backup if the main file is broken; null when there is none,
    // or when the save is from a newer version of the game (Continue stays greyed out, no .bak fallback).
    private SaveData ReadSave()
    {
        foreach (string path in new[] { savePath, savePath + ".bak" })
        {
            try
            {
                SaveData data = SaveFile.Read(path);
                if (data == null) continue;

                if (data.version > SaveData.CurrentVersion)
                {
                    Debug.LogWarning($"{path} is from a newer version of the game (save version {data.version}).", this);
                    return null;
                }

                return data;
            }
            catch (System.Exception e) when (e is System.IO.IOException || e is System.ArgumentException || e is System.IO.InvalidDataException)
            {
                Debug.LogWarning($"Could not read {path}: {e.Message}", this);
            }
        }

        return null;
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
        settingsPanel.gameObject.SetActive(show);
        controlsPanel.SetActive(false);
        ShowMenuButtons(!show);
    }

    private void OpenControls()
    {
        ShowMenuButtons(false);
        controlsPanel.SetActive(true);
        controlsBackButton.Select();
    }

    private void CloseControls()
    {
        controlsPanel.SetActive(false);
        ShowMenuButtons(true);
        helpButton.Select();
    }

    private void ShowMenuButtons(bool visible)
    {
        helpButton.gameObject.SetActive(visible);
        newGameButton.gameObject.SetActive(visible);
        continueButton.gameObject.SetActive(visible);
        settingsButton.gameObject.SetActive(visible);
        exitButton.gameObject.SetActive(visible);

        // Leaving and coming back resets the "Start over?" question.
        confirmingNewGame = false;
        newGameLabel.text = newGameText;
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
