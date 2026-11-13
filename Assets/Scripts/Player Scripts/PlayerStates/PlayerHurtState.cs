using UnityEngine;

// Got hit: short knockback, no control, and invincible for a moment so enemies can't stun-lock the player.
// Stunned for hurtDuration (the Hurt clip) or the knockback, whichever is longer.
// -> Idle when the stun ends, -> Dead when health reaches 0
public class PlayerHurtState : PlayerState
{
    private float knockbackTimer;
    private float stunTimer;

    public PlayerHurtState(Player player) : base(player) { }

    public override void Enter()
    {
        knockbackTimer = player.lastDamage.knockbackDuration;
        stunTimer = Mathf.Max(player.playerData.hurtDuration, knockbackTimer);
        player.playerHealth.StartInvincibility(player.playerData.invincibleTimer);
        player.playerAnimator.SetAnimationState(PlayerAnimationState.Hurt);
    }

    public override void Update()
    {
        knockbackTimer -= Time.deltaTime;
        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
            player.playerStateMachine.ChangeState(player.idleState);
    }

    public override void FixedUpdate()
    {
        if (knockbackTimer > 0f)
            player.playerMotor.Move(player.lastDamage.direction, player.lastDamage.knockback);
        else
            player.playerMotor.Stop();
    }

    public override void Exit()
    {
        player.playerMotor.Stop();
    }
}
