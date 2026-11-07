using UnityEngine;

// Walking to the next patrol point. -> Chase when a target is spotted, -> Idle on arrival (waits, then the next point)
public class EnemyPatrolState : EnemyState
{
    public EnemyPatrolState(Enemy enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.enemyAnimator.SetAnimationState(EnemyAnimationState.Walk);
    }

    public override void Update()
    {
        if (enemy.target != null)
        {
            enemy.enemyStateMachine.ChangeState(enemy.chaseState);
            return;
        }

        if (enemy.ReachedPatrolPoint())
        {
            enemy.AdvancePatrolPoint();
            enemy.enemyStateMachine.ChangeState(enemy.idleState);
            return;
        }

        enemy.SetFacingDirection(enemy.patrolDestination - (Vector2)enemy.transform.position);
    }

    public override void FixedUpdate()
    {
        enemy.enemyMotor.MoveTowards(enemy.patrolDestination, enemy.enemyData.walkSpeed);
    }

    public override void Exit()
    {
        enemy.enemyMotor.Stop();
    }
}
