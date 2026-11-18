using UnityEngine;

// Short burst of speed in the facing direction (Walk and Run keep it equal to the move input).
// Accepts no input; the player can't be hurt while dashing. Has a cooldown, checked through canDash.
// -> Idle when the dash time is over (Idle then picks Walk or Run from the input)
public class PlayerDashState : PlayerState
{
    private Vector2 dashDirection;
    private float dashTimer;
    private float nextDashTime;

    public PlayerDashState(Player player) : base(player) { }

    public bool canDash => Time.time >= nextDashTime;

    public override void Enter()
    {
        dashDirection = player.facingDirection.normalized;
        dashTimer = player.playerData.dashDuration;

        player.playerHealth.StartInvincibility(player.playerData.dashDuration);
        player.playerAnimator.SetAnimationState(PlayerAnimationState.Dash);
    }

    public override void Update()
    {
        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0f)
            player.playerStateMachine.ChangeState(player.idleState);
    }

    public override void FixedUpdate()
    {
        player.playerMotor.Move(dashDirection, player.playerData.dashSpeed);
    }

    public override void Exit()
    {
        player.playerMotor.Stop();
        nextDashTime = Time.time + player.playerData.dashCooldown;
    }
}
