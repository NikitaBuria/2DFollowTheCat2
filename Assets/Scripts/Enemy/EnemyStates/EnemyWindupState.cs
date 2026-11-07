using UnityEngine;

// The telegraph before the hit: the enemy stops and locks its facing, so the player sees it coming
// and can step out of range. -> Attack when windupTime is over, -> Idle when the target is lost
public class EnemyWindupState : EnemyState
{
    private float windupTimer;

    public EnemyWindupState(Enemy enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.enemyMotor.Stop();
        enemy.FaceTarget();
        enemy.enemyAnimator.SetAnimationState(EnemyAnimationState.Windup);
        windupTimer = enemy.enemyData.windupTime;
    }

    public override void Update()
    {
        if (enemy.target == null)
        {
            enemy.enemyStateMachine.ChangeState(enemy.idleState);
            return;
        }

        windupTimer -= Time.deltaTime;
        if (windupTimer <= 0f)
            enemy.enemyStateMachine.ChangeState(enemy.attackState);
    }

    public override void Exit()
    {
    }
}
