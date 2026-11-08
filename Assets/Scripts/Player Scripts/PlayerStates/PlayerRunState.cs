using UnityEngine;

// Moving at run speed while sprint is held.
// -> Idle when no input, -> Walk when sprint released, -> Dash, -> Attack, -> Hurt
public class PlayerRunState : PlayerState
{
    private Vector2 moveInput;

    public PlayerRunState(Player player) : base(player) { }

    public override void Enter()
    {
        player.playerAnimator.SetAnimationState(PlayerAnimationState.Run);
    }

    public override void Update()
    {
        if (player.playerInputReader.attackPressedThisFrame)
        {
            player.playerStateMachine.ChangeState(player.attackState);
            return;
        }

        moveInput = player.playerInputReader.moveInput;

        if (moveInput == Vector2.zero)
        {
            player.playerStateMachine.ChangeState(player.idleState);
            return;
        }

        player.SetFacingDirection(moveInput);

        if (!player.playerInputReader.isSprintHeld)
            player.playerStateMachine.ChangeState(player.walkState);
    }

    public override void FixedUpdate()
    {
        player.playerMotor.Move(moveInput, player.playerData.runSpeed);
    }

    public override void Exit()
    {
    }
}
