using UnityEngine;

// How the player hits things. The stats come from the equipped WeaponData;
// PlayerAttackState decides when to call HandleAttack.
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private WeaponData currentWeapon;
    [SerializeField] private LayerMask enemyLayerMask;

    public WeaponData CurrentWeapon => currentWeapon;

    // Remembered only to draw the gizmo where the last attack happened.
    private Vector2 lastAttackDirection = Vector2.down;

    private void Awake()
    {
        if (currentWeapon == null)
            Debug.LogError($"{name}: PlayerCombat has no weapon assigned.", this);

        // Fallback so the mask is never "Nothing" if it wasn't set in the Inspector.
        if (enemyLayerMask == 0) enemyLayerMask = LayerMask.GetMask("Enemy");
    }

    // Hits every damageable enemy inside the weapon's circle and arc, once.
    public void HandleAttack(Vector2 direction)
    {
        if (currentWeapon == null) return;

        direction = direction.normalized;
        lastAttackDirection = direction;

        Vector2 origin = transform.position;
        Vector2 center = GetAttackCenter(origin, direction);

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, currentWeapon.AttackRange, enemyLayerMask);

        foreach (Collider2D hit in hitColliders)
        {
            Vector2 toTarget = (Vector2)hit.transform.position - origin;

            // Outside the cone in front of the player -> skip.
            if (Vector2.Angle(direction, toTarget) > currentWeapon.AttackArc / 2f) continue;

            if (hit.TryGetComponent(out IDamageable target) && target.IsAlive)
            {
                target.TakeDamage(new DamageData(
                    currentWeapon.Damage,
                    toTarget,
                    currentWeapon.Knockback,
                    currentWeapon.KnockbackDuration));
            }
        }
    }

    private Vector2 GetAttackCenter(Vector2 origin, Vector2 direction)
    {
        return origin + direction * currentWeapon.AttackRange;
    }

    private void OnDrawGizmosSelected()
    {
        if (currentWeapon == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackCenter(transform.position, lastAttackDirection.normalized), currentWeapon.AttackRange);
    }
}
