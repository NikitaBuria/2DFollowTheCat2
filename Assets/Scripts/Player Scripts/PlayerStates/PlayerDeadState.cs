using UnityEngine;

// Health is 0. No transitions out (restart or game over is handled elsewhere).
public class PlayerDeadState : PlayerState
{
    public PlayerDeadState(Player player) : base(player) { }

    public override void Enter()
    {
        player.playerAnimator.SetAnimationState(PlayerAnimationState.Dead);
        player.playerMotor.Stop();
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        playerCollider.enabled=false;
    }

    public override void Update()
    {
        // Later: respawn at a checkpoint (back to Idle). For now the player stays dead until the scene is reloaded.
    }


    public override void FixedUpdate()
    {
        player.playerMotor.Stop();
    }

    public override void Exit()
    {
    }



    
}
