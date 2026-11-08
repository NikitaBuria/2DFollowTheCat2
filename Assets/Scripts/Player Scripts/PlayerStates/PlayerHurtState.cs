using UnityEngine;

// Got hit: short knockback, no control, and invincible for a moment so enemies can't stun-lock the player.
// -> Idle when knockback ends, -> Dead when health reaches 0
public class PlayerHurtState : PlayerState
{
    private float knockBackTimer;

    public PlayerHurtState(Player player) : base(player) { }

    public override void Enter()
    {
        knockBackTimer = player.lastDamage.knockbackDuration;
        player.playerHealth.StartInvincibility(player.playerData.invincibleTimer);
        player.playerAnimator.SetAnimationState(PlayerAnimationState.Hurt);
    }

    public override void Update()
    {
        knockBackTimer -= Time.deltaTime;

        if (knockBackTimer <= 0f)
            player.playerStateMachine.ChangeState(player.idleState);
    }

    public override void FixedUpdate()
    {
        if (knockBackTimer > 0f)
            player.playerMotor.Move(player.lastDamage.direction, player.lastDamage.knockback);
        else
            player.playerMotor.Stop();
    }

    public override void Exit()
    {
        player.playerMotor.Stop();
    }
}
