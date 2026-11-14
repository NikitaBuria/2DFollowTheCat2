using UnityEngine;

// Health reached 0. No way out of its own: stops, turns off its colliders, drops its loot and despawns
// after destroyDelay (destroyed, or back to its pool). A pooled enemy leaves through ResetForSpawn -> Idle.
public class EnemyDeadState : EnemyState
{
    private float despawnTimer;
    private bool hasDespawned;

    public EnemyDeadState(Enemy enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.enemyMotor.Stop();
        enemy.enemyDetection.Clear();
        enemy.enemyAnimator.SetAnimationState(EnemyAnimationState.Dead);

        // A corpse shouldn't block the player or be hit again.
        SetCollidersEnabled(false);

        // Enter runs once, so the loot drops once per death.
        if (enemy.enemyData.lootTable != null)
            enemy.enemyData.lootTable.DropLoot(enemy.transform.position);

        despawnTimer = enemy.enemyData.destroyDelay;
        hasDespawned = false;
    }

    public override void Update()
    {
        if (hasDespawned) return;

        despawnTimer -= Time.deltaTime;
        if (despawnTimer > 0f) return;

        hasDespawned = true;
        enemy.Despawn();
    }

    public override void Exit()
    {
        // Only a pooled enemy leaves Dead; it needs its colliders back.
        SetCollidersEnabled(true);
    }

    private void SetCollidersEnabled(bool isEnabled)
    {
        foreach (Collider2D enemyCollider in enemy.GetComponentsInChildren<Collider2D>(true))
            enemyCollider.enabled = isEnabled;
    }
}
