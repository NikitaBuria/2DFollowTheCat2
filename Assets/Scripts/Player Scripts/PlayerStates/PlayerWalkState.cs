using UnityEngine;

// Moving at walk speed.
// -> Idle when no input, -> Run while sprint held, -> Dash, -> Attack, -> Hurt
public class PlayerWalkState : PlayerState
{
    private Vector2 moveInput;

    public PlayerWalkState(Player player) : base(player) { }

    public override void Enter()
    {
        // play walk animation
    }

    public override void Update()
    {
        moveInput = player.playerInputReader.MoveInput;

        if (moveInput == Vector2.zero)
        {
            player.playerStateMachine.ChangeState(player.IdleState);
            return;
        }

        if (player.playerInputReader.IsSprintHeld)
            player.playerStateMachine.ChangeState(player.RunState);
    }

    public override void FixedUpdate()
    {
        player.playerMotor.Move(moveInput, player.WalkSpeed);
    }

    public override void Exit()
    {
    }
}
