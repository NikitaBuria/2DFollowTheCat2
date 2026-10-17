using UnityEngine;

// Health is 0. No transitions out (restart or game over is handled elsewhere).
public class PlayerDeadState : PlayerState
{
    public PlayerDeadState(Player player) : base(player) { }

    public override void Enter()
    {
        // stop movement, disable input, play death animation
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
    }
}
