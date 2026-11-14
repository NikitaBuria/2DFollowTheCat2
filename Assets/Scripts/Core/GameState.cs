// The mode the whole game is in. GameStateManager owns the current value.
public enum GameState
{
    Gameplay,
    Inventory,
    Paused,
    Shop,
    // Every town building is restored; the win panel is open.
    Won
}
