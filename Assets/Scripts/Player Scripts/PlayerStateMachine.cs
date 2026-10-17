
using System;


public sealed class PlayerStateMachine 
{
    public PlayerState CurrentState{get; private set;}


                         //old state   //new state    
    public event Action <PlayerState, PlayerState> StateChanged;

    public void Initialize(PlayerState initialState)
    {
        if (CurrentState != null) throw new InvalidOperationException("Already initialized");
        ChangeState(initialState);
    }


    public void ChangeState(PlayerState nextState)
    {
        
            if (nextState == null) throw new ArgumentNullException(nameof(nextState));

            if (nextState.Equals(CurrentState)) return;

            PlayerState previous = CurrentState;
            previous?.Exit();
            CurrentState = nextState;
            StateChanged?.Invoke(previous,nextState);        
        
    }

    public void ShutDown()
    {
        if(CurrentState==null) return;
        PlayerState previous = CurrentState;
        previous.Exit();
        CurrentState = null;
        StateChanged?.Invoke(previous,null);
        
    }
    
}
