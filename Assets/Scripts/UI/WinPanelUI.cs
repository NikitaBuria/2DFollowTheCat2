using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The "thanks for playing" panel. UIManager shows it in GameState.Won; this script only handles its buttons.
public class WinPanelUI : MonoBehaviour
{
    private const string MainMenuScene = "MainMenu";

    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;

    private void OnEnable()
    {
        continueButton.onClick.AddListener(Continue);
        exitButton.onClick.AddListener(Exit);
        // Keyboard / controller can press it right away.
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
    }

    private void OnDisable()
    {
        continueButton.onClick.RemoveListener(Continue);
        exitButton.onClick.RemoveListener(Exit);
    }

    // Back to the town; the boss can still be farmed.
    private void Continue()
    {
        gameStateManager.ChangeState(GameState.Gameplay);
    }

    // The restore already saved, so nothing is lost. Time runs again before the menu loads.
    private void Exit()
    {
        gameStateManager.ChangeState(GameState.Gameplay);
        SceneLoader.Load(MainMenuScene, null);
    }
}
