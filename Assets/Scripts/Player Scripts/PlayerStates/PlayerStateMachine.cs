using System;

// Holds the current player state. Only this class switches states: old Exit -> swap -> new Enter.
public sealed class PlayerStateMachine
{
    public PlayerState currentState { get; private set; }

    public void Initialize(PlayerState initialState)
    {
        if (currentState != null) throw new InvalidOperationException("Already initialized");
        ChangeState(initialState);
    }

    public void ChangeState(PlayerState nextState)
    {
        if (nextState == null) throw new ArgumentNullException(nameof(nextState));
        if (nextState == currentState) return;

        currentState?.Exit();
        currentState = nextState;
        currentState.Enter();
    }
}
