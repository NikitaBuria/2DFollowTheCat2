using UnityEngine;

// Standing still, no input.
// -> Walk when move input, -> Dash on dash press, -> Attack on attack press, -> Hurt when hit
public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(Player player) : base(player) { }

    public override void Enter()
    {
        // stop movement, play idle animation
    }

    public override void Update()
    {
        // check input and switch state
    }

    public override void Exit()
    {
    }
}
