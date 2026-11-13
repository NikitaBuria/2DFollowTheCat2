using UnityEngine;

// Standing still, no input.
// -> Walk when move input, -> Dash on dash press, -> Attack on attack press, -> Hurt when hit
public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(Player player) : base(player) { }

    public override void Enter()
    {
        player.playerMotor.Stop();
        player.playerAnimator.SetAnimationState(PlayerAnimationState.Idle);
    }

    public override void Update()
    {
        HandleHotbarInput();
        HandleInteractInput();

        if (player.playerInputReader.attackPressedThisFrame)
        {
            player.playerStateMachine.ChangeState(player.attackState);
            return;
        }

        if (player.playerInputReader.moveInput != Vector2.zero)
            player.playerStateMachine.ChangeState(player.walkState);
    }

    public override void FixedUpdate()
    {
        player.playerMotor.Stop();
    }

    public override void Exit()
    {
    }
}
