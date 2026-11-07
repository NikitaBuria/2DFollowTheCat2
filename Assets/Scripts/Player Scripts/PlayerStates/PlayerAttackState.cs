// Attacking; movement is locked until the attack finishes.
// -> Idle or Walk when the attack ends, -> Hurt when hit

using UnityEngine;
public class PlayerAttackState : PlayerState
{
    public PlayerAttackState(Player player) : base(player) { }

    private float attackTimer;


    public override void Enter()
    {
        player.playerMotor.Stop();
        attackTimer = player.playerCombat.CurrentWeapon != null
            ? player.playerCombat.CurrentWeapon.AttackDuration
            : 0f;
        player.playerAnimator.SetAnimationState(PlayerAnimationState.Attack);

        // Hit once per attack. Calling this in Update would hit every frame.
        player.playerCombat.HandleAttack(player.FacingDirection);
    }

    public override void Update()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        if (player.playerInputReader.MoveInput != Vector2.zero)
            player.playerStateMachine.ChangeState(player.WalkState);
        else
            player.playerStateMachine.ChangeState(player.IdleState);
    }

    public override void Exit()
    {
    }
}
