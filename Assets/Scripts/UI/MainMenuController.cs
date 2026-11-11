using UnityEngine;
using UnityEngine.UI;

// The main menu scene (Astra's MainMenu prefab). Play opens the first game scene, whose SaveManager
// loads the save if there is one, so Play works as both "New Game" and "Continue".
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [Tooltip("Scene name as listed in Build Profiles > Scene List.")]
    [SerializeField] private string firstScene = "Town";

    private void Awake()
    {
        // No settings menu yet (step 10): show the button but don't let it do nothing.
        settingsButton.interactable = false;
    }

    private void OnEnable()
    {
        playButton.onClick.AddListener(Play);
        exitButton.onClick.AddListener(Exit);
    }

    private void OnDisable()
    {
        playButton.onClick.RemoveListener(Play);
        exitButton.onClick.RemoveListener(Exit);
    }

    private void Play()
    {
        // No spawn ID: the player starts at the scene's own PlayerSpawnPoint.
        SceneLoader.Load(firstScene, null);
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
