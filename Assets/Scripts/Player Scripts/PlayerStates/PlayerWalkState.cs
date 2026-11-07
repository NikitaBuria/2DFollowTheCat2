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
        if (player.playerInputReader.AttackPressedThisFrame)
        {
            player.playerStateMachine.ChangeState(player.AttackState);
            return;
        } 

        moveInput = player.playerInputReader.MoveInput;
    


        if (moveInput == Vector2.zero)
        {
            player.playerStateMachine.ChangeState(player.IdleState);
            return;
        }

        player.SetFacingDirection(moveInput);

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
