using System;

public sealed class PlayerStateMachine
{
    public PlayerState currentState { get; private set; }

    // old state, new state
    public event Action<PlayerState, PlayerState> OnStateChanged;

    public void Initialize(PlayerState initialState)
    {
        if (currentState != null) throw new InvalidOperationException("Already initialized");
        ChangeState(initialState);
    }

    public void ChangeState(PlayerState nextState)
    {
        if (nextState == null) throw new ArgumentNullException(nameof(nextState));
        if (nextState == currentState) return;

        PlayerState previous = currentState;
        previous?.Exit();
        currentState = nextState;
        currentState.Enter();

        OnStateChanged?.Invoke(previous, nextState);
    }

    public void Shutdown()
    {
        if (currentState == null) return;

        PlayerState previous = currentState;
        previous.Exit();
        currentState = null;

        OnStateChanged?.Invoke(previous, null);
    }
}
