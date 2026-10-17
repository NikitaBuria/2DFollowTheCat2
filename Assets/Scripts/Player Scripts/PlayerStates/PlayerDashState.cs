using UnityEngine;

// Short burst of speed in the move direction, lasts a fixed time.
// -> Idle or Walk when the dash time is over
public class PlayerDashState : PlayerState
{
    public PlayerDashState(Player player) : base(player) { }

    public override void Enter()
    {
        // apply dash velocity, start dash timer, play dash animation
    }

    public override void Update()
    {
        // count down the timer, leave the state when it ends
    }

    public override void Exit()
    {
        // start dash cooldown
    }
}
