using System;
using UnityEngine;

// Shows and hides the menu panels to match the game state. Only visibility (and where the inventory
// panel sits); each panel's content is drawn by its own script (CoinCounterUI, InventoryUI...).
// Also the only UI script that knows the game state for world-space text: it hides the interaction
// prompt and announces OnWorldUIVisibilityChanged for the speech bubbles while a menu is open.
// Sits on the HUD, which stays active, so it keeps listening while the panels are off.
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject merchantPanel;
    [SerializeField] private GameObject winPanel;
    [Tooltip("The world-space [E] prompt (InteractionPromptView). Shown only in Gameplay.")]
    [SerializeField] private GameObject interactionPrompt;
    [Tooltip("Inventory panel position when only the inventory is open. In the shop it keeps its own layout position, beside the merchant panel.")]
    [SerializeField] private Vector2 inventoryOnlyPosition = Vector2.zero;

    // Speech bubbles live on the NPCs in each scene, outside the HUD, so they can't be wired here.
    // They listen to this instead (static, like Enemy.OnAnyEnemyDied): true = no menu covers the world.
    public static event Action<bool> OnWorldUIVisibilityChanged;
    public static bool isWorldUIVisible { get; private set; } = true;

    private RectTransform inventoryRect;
    private Vector2 shopInventoryPosition;

    private void Awake()
    {
        inventoryRect = (RectTransform)inventoryPanel.transform;
        shopInventoryPosition = inventoryRect.anchoredPosition;
    }

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
        inventoryRect.anchoredPosition = gameState == GameState.Inventory ? inventoryOnlyPosition : shopInventoryPosition;
        inventoryPanel.SetActive(gameState == GameState.Inventory || gameState == GameState.Shop);
        merchantPanel.SetActive(gameState == GameState.Shop);

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(gameState == GameState.Paused);
        if (winPanel != null) winPanel.SetActive(gameState == GameState.Won);

        // World-space text only while playing, never on top of (or peeking out behind) a menu.
        bool worldUIVisible = gameState == GameState.Gameplay;
        interactionPrompt.SetActive(worldUIVisible);
        isWorldUIVisible = worldUIVisible;
        OnWorldUIVisibilityChanged?.Invoke(worldUIVisible);
    }
}
