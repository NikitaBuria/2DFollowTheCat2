using UnityEngine;

// Hits once on Enter (misses if the target dodged during the windup), then recovers.
// The recovery is the player's chance to strike back. -> Chase or Idle when recoverTime is over
public class EnemyAttackState : EnemyState
{
    private float recoverTimer;

    public EnemyAttackState(Enemy enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.enemyAnimator.SetAnimationState(EnemyAnimationState.Attack);
        enemy.enemyCombat.PerformAttack(enemy.target);
        recoverTimer = enemy.enemyData.recoverTime;
    }

    public override void Update()
    {
        recoverTimer -= Time.deltaTime;
        if (recoverTimer > 0f) return;

        if (enemy.target != null)
            enemy.enemyStateMachine.ChangeState(enemy.chaseState);
        else
            enemy.enemyStateMachine.ChangeState(enemy.idleState);
    }

    public override void Exit()
    {
    }
}
