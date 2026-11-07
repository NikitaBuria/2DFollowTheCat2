using UnityEngine;

// Attacking; movement is locked until the attack finishes.
// -> Idle or Walk when the attack ends, -> Hurt when hit
public class PlayerAttackState : PlayerState
{
    private float attackTimer;

    public PlayerAttackState(Player player) : base(player) { }

    public override void Enter()
    {
        player.playerMotor.Stop();
        attackTimer = player.playerCombat.currentWeapon != null ? player.playerCombat.currentWeapon.attackDuration : 0f;

        player.playerAnimator.SetAnimationState(PlayerAnimationState.Attack);

        // Hit once per attack. Calling this in Update would hit every frame.
        player.playerCombat.HandleAttack(player.facingDirection);
    }

    public override void Update()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        if (player.playerInputReader.moveInput != Vector2.zero)
            player.playerStateMachine.ChangeState(player.walkState);
        else
            player.playerStateMachine.ChangeState(player.idleState);
    }

    public override void Exit()
    {
    }
}
