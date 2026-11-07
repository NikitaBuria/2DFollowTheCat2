using UnityEngine;

// Moving at walk speed.
// -> Idle when no input, -> Run while sprint held, -> Dash, -> Attack, -> Hurt
public class PlayerWalkState : PlayerState
{
    private Vector2 moveInput;

    public PlayerWalkState(Player player) : base(player) { }

    public override void Enter()
    {
        player.playerAnimator.SetAnimationState(PlayerAnimationState.Walk);
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

        if (player.playerInputReader.isSprintHeld)
            player.playerStateMachine.ChangeState(player.runState);
    }

    public override void FixedUpdate()
    {
        player.playerMotor.Move(moveInput, player.characterData.walkSpeed);
    }

    public override void Exit()
    {
    }
}
