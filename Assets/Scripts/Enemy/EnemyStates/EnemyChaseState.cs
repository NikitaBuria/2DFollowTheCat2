// Running at the target. -> Windup when the target is in weapon range, -> Idle when the target is lost
public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(Enemy enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.enemyAnimator.SetAnimationState(EnemyAnimationState.Run);
    }

    public override void Update()
    {
        if (enemy.target == null)
        {
            enemy.enemyStateMachine.ChangeState(enemy.idleState);
            return;
        }

        // A boss may have something better to do (summon).
        EnemyState special = enemy.GetSpecialState();
        if (special != null)
        {
            enemy.enemyStateMachine.ChangeState(special);
            return;
        }

        if (enemy.enemyCombat.IsInRange(enemy.target))
        {
            enemy.enemyStateMachine.ChangeState(enemy.windupState);
            return;
        }

        enemy.FaceTarget();
    }

    public override void FixedUpdate()
    {
        if (enemy.target != null)
            enemy.enemyMotor.MoveTowards(enemy.target.position, enemy.enemyData.runSpeed);
    }

    public override void Exit()
    {
        enemy.enemyMotor.Stop();
    }
}
