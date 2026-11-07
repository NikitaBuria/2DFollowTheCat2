using UnityEngine;

// Standing still. -> Chase when a target is spotted, -> Patrol after patrolWaitTime (only with a patrol route)
public class EnemyIdleState : EnemyState
{
    private float waitTimer;

    public EnemyIdleState(Enemy enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.enemyMotor.Stop();
        enemy.enemyAnimator.SetAnimationState(EnemyAnimationState.Idle);
        waitTimer = enemy.enemyData.patrolWaitTime;
    }

    public override void Update()
    {
        if (enemy.target != null)
        {
            enemy.enemyStateMachine.ChangeState(enemy.chaseState);
            return;
        }

        if (!enemy.hasPatrolRoute) return;

        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
            enemy.enemyStateMachine.ChangeState(enemy.patrolState);
    }

    public override void Exit()
    {
    }
}
