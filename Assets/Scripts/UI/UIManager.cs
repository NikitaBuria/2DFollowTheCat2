using UnityEngine;

// Shows and hides the menu panels to match the game state. Only visibility; each panel's
// content is drawn by its own script (CoinCounterUI, InventoryUI...).
// Sits on the HUD, which stays active, so it keeps listening while the panels are off.
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject merchantPanel;

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
        // The shop shows the inventory too, so items can be dragged onto the merchant panel to sell.
        inventoryPanel.SetActive(gameState == GameState.Inventory || gameState == GameState.Shop);
        merchantPanel.SetActive(gameState == GameState.Shop);

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(gameState == GameState.Paused);
    }
}
