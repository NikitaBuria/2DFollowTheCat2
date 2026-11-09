using System;
using UnityEngine;

// Single source of truth for the game's mode (Gameplay, Inventory, Paused).
// Polls the menu keys, sets Time.timeScale and the Player action map, and announces the change.
public class GameStateManager : MonoBehaviour
{
    [SerializeField] private PlayerInputReader playerInputReader;

    public GameState currentState { get; private set; }

    public event Action<GameState> OnGameStateChanged;

    private void Update()
    {
        bool inventoryPressed = playerInputReader.inventoryPressedThisFrame;
        bool menuPressed = playerInputReader.menuPressedThisFrame;

        switch (currentState)
        {
            case GameState.Gameplay:
                if (menuPressed) ChangeState(GameState.Paused);
                else if (inventoryPressed) ChangeState(GameState.Inventory);
                break;

            case GameState.Inventory:
                // The menu key acts as "back" here: it closes the inventory instead of pausing.
                if (inventoryPressed || menuPressed) ChangeState(GameState.Gameplay);
                break;

            case GameState.Paused:
                if (menuPressed) ChangeState(GameState.Gameplay);
                break;
        }
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        Time.timeScale = newState switch
        {
            GameState.Gameplay => 1f,
            GameState.Inventory => 0f,
            GameState.Paused => 0f,
            _ => throw new ArgumentOutOfRangeException(nameof(newState), newState, null)
        };

        currentState = newState;

        if (currentState == GameState.Gameplay) playerInputReader.EnablePlayerActions();
        else playerInputReader.DisablePlayerActions();

        OnGameStateChanged?.Invoke(currentState);
    }
}
