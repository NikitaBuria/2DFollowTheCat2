using UnityEngine;

// Shows and hides the menu panels to match the game state. Only visibility; each panel's
// content is drawn by its own script (CoinCounterUI, InventoryUI...).
// Sits on the HUD, which stays active, so it keeps listening while the panels are off.
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject pauseMenuPanel;

    private void OnEnable()
    {
        gameStateManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        gameStateManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Start()
    {
        HandleGameStateChanged(gameStateManager.currentState);
    }

    private void HandleGameStateChanged(GameState gameState)
    {
        inventoryPanel.SetActive(gameState == GameState.Inventory);

        // Optional until the pause menu exists.
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(gameState == GameState.Paused);
    }
}
