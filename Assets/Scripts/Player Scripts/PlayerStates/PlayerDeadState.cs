using UnityEngine;

// Health is 0. No transitions out of its own: RespawnManager calls Player.Respawn, which goes to Idle.
public class PlayerDeadState : PlayerState
{
    private readonly Collider2D playerCollider;

    public PlayerDeadState(Player player) : base(player)
    {
        playerCollider = player.GetComponent<Collider2D>();
    }

    public override void Enter()
    {
        player.playerAnimator.SetAnimationState(PlayerAnimationState.Dead);
        player.playerMotor.Stop();
        playerCollider.enabled = false;
    }

    public override void Update()
    {
    }

    public override void FixedUpdate()
    {
        player.playerMotor.Stop();
    }

    // Leaving Dead only happens on respawn.
    public override void Exit()
    {
        playerCollider.enabled = true;
    }
}
