using UnityEngine;

// How the enemy hits (with its body, no weapon). The stats come from EnemyData;
// the windup and attack states decide when to call PerformAttack.
public class EnemyCombat : MonoBehaviour
{
    private EnemyData enemyData;

    public void Initialize(EnemyData data)
    {
        enemyData = data;
    }

    // Measures to the closest point of the target's collider, so big targets are reached at their edge.
    public bool IsInRange(Transform target)
    {
        if (enemyData == null || target == null) return false;

        Vector2 origin = transform.position;
        Vector2 closestPoint = target.TryGetComponent(out Collider2D targetCollider)
            ? targetCollider.ClosestPoint(origin)
            : (Vector2)target.position;

        // sqrMagnitude skips the square root: comparing squared distances gives the same answer.
        float range = enemyData.attackRange;
        return (closestPoint - origin).sqrMagnitude <= range * range;
    }

    // Checks the range again at the moment of the hit: the target may have dodged during the windup.
    public void PerformAttack(Transform target)
    {
        if (!IsInRange(target)) return;
        if (!target.TryGetComponent(out IDamageable damageable) || !damageable.isAlive) return;

        Vector2 toTarget = target.position - transform.position;
        damageable.TakeDamage(new DamageData(
            enemyData.attackDamage,
            toTarget,
            enemyData.knockback,
            enemyData.knockbackDuration));
    }
}
