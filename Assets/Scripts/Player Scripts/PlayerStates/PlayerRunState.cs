using UnityEngine;

// Moving at run speed while sprint is held.
// -> Idle when no input, -> Walk when sprint released, -> Dash, -> Attack, -> Hurt
public class PlayerRunState : PlayerState
{
    private Vector2 moveInput;

    public PlayerRunState(Player player) : base(player) { }

    public override void Enter()
    {
        // play run animation
    }

    public override void Update()
    {
        moveInput = player.playerInputReader.MoveInput;

        if (moveInput == Vector2.zero)
        {
            player.playerStateMachine.ChangeState(player.IdleState);
            return;
        }

        if (!player.playerInputReader.IsSprintHeld)
            player.playerStateMachine.ChangeState(player.WalkState);
    }

    public override void FixedUpdate()
    {
        player.playerMotor.Move(moveInput, player.RunSpeed);
    }

    public override void Exit()
    {
    }
}
