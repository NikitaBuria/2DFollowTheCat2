using UnityEngine;

// Standing still, no input.
// -> Walk when move input, -> Dash on dash press, -> Attack on attack press, -> Hurt when hit
public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(Player player) : base(player) { }

    public override void Enter()
    {
        player.playerMotor.Stop();
    }

    public override void Update()
    {
        if (player.playerInputReader.MoveInput != Vector2.zero)
            player.playerStateMachine.ChangeState(player.WalkState);
    }

    public override void Exit()
    {
    }
}
