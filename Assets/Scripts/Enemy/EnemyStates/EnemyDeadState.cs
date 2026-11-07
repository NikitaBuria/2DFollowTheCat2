using UnityEngine;

// Health reached 0. No way out: stops, turns off its colliders and removes itself after destroyDelay.
// Later (Loot step): drop loot here.
public class EnemyDeadState : EnemyState
{
    public EnemyDeadState(Enemy enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.enemyMotor.Stop();
        enemy.enemyDetection.Clear();
        enemy.enemyAnimator.SetAnimationState(EnemyAnimationState.Dead);

        // A corpse shouldn't block the player or be hit again.
        foreach (Collider2D enemyCollider in enemy.GetComponentsInChildren<Collider2D>())
            enemyCollider.enabled = false;

        Object.Destroy(enemy.gameObject, enemy.enemyData.destroyDelay);
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
    }
}
