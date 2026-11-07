using UnityEngine;

// Got hit: knocked back and stunned for a moment. Being hit again while hurt doesn't restart it,
// so the player can't stun-lock an enemy forever. -> Chase or Idle when the stun ends
public class EnemyHurtState : EnemyState
{
    private float stunTimer;
    private float knockbackTimer;

    public EnemyHurtState(Enemy enemy) : base(enemy) { }

    public override void Enter()
    {
        DamageData damage = enemy.lastDamage;

        enemy.enemyAnimator.SetAnimationState(EnemyAnimationState.Hurt);
        knockbackTimer = damage.knockbackDuration;
        stunTimer = Mathf.Max(enemy.enemyData.hurtDuration, damage.knockbackDuration);
    }

    public override void Update()
    {
        knockbackTimer -= Time.deltaTime;
        stunTimer -= Time.deltaTime;
        if (stunTimer > 0f) return;

        // Getting hit makes the enemy fight back if it can see someone.
        if (enemy.target != null)
            enemy.enemyStateMachine.ChangeState(enemy.chaseState);
        else
            enemy.enemyStateMachine.ChangeState(enemy.idleState);
    }

    public override void FixedUpdate()
    {
        if (knockbackTimer > 0f)
            enemy.enemyMotor.Move(enemy.lastDamage.direction, enemy.lastDamage.knockback);
        else
            enemy.enemyMotor.Stop();
    }

    public override void Exit()
    {
        enemy.enemyMotor.Stop();
    }
}
