using System;

// Holds the current enemy state. Only this class switches states: old Exit -> swap -> new Enter.
public sealed class EnemyStateMachine
{
    public EnemyState currentState { get; private set; }

    // old state, new state
    public event Action<EnemyState, EnemyState> OnStateChanged;

    public void Initialize(EnemyState initialState)
    {
        if (currentState != null) throw new InvalidOperationException("Already initialized");
        ChangeState(initialState);
    }

    public void ChangeState(EnemyState nextState)
    {
        if (nextState == null) throw new ArgumentNullException(nameof(nextState));
        if (nextState == currentState) return;

        EnemyState previous = currentState;
        previous?.Exit();
        currentState = nextState;
        currentState.Enter();

        OnStateChanged?.Invoke(previous, nextState);
    }
}
