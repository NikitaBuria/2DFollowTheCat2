using System.Collections.Generic;
using UnityEngine;

// How the player hits things. The stats come from the equipped WeaponData;
// PlayerAttackState decides when to call HandleAttack.
public class PlayerCombat : MonoBehaviour
{
    [field: SerializeField] public WeaponData currentWeapon { get; private set; }
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private Transform attackPoint;

    // Targets already hit during the current swing. Reused every attack so it doesn't create garbage.
    private readonly HashSet<IDamageable> hitThisSwing = new();

    private void Awake()
    {
        if (currentWeapon == null)
            Debug.LogError($"{name}: PlayerCombat has no weapon assigned.", this);

        if (attackPoint == null)
            Debug.LogError($"{name}: PlayerCombat has no attackPoint assigned.", this);

        // "Nothing" means the attack can never hit anything, so say so instead of failing silently.
        if (enemyLayerMask == 0)
            Debug.LogError($"{name}: PlayerCombat's enemyLayerMask is Nothing. Pick the Enemy layer in the Inspector.", this);
    }

    // Hits every damageable enemy inside the weapon's range and arc, once per swing.
    public void HandleAttack(Vector2 direction)
    {
        if (currentWeapon == null || attackPoint == null) return;

        direction = direction.normalized;
        Vector2 origin = attackPoint.position;

        // Circle centered on the attack point: attackRange is the real reach.
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin, currentWeapon.attackRange, enemyLayerMask);

        hitThisSwing.Clear();

        foreach (Collider2D hit in hitColliders)
        {
            // GetComponentInParent also finds the script when the collider sits on a child object.
            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target == null || !target.isAlive) continue;

            Vector2 toTarget = hit.ClosestPoint(origin) - origin;
            if (toTarget == Vector2.zero) toTarget = direction; // If the closest point is exactly at the origin, use the attack direction instead.

            // Outside the cone in front of the player -> skip.
            if (Vector2.Angle(direction, toTarget) > currentWeapon.attackArc / 2f) continue;

            // Add returns false if this enemy was already hit (e.g. it has two colliders).
            if (!hitThisSwing.Add(target)) continue;

            target.TakeDamage(new DamageData(
                currentWeapon.damage,
                toTarget,
                currentWeapon.knockback,
                currentWeapon.knockbackDuration));
        }
    }

    // Draws the hit area (a "pizza slice") while the Player is selected:
    // faint circle = full OverlapCircleAll range, red slice = the part that actually hits.
    // Editor-only debug drawing, so reading Player here doesn't couple the game logic to it.
    private void OnDrawGizmosSelected()
    {
        if (currentWeapon == null || attackPoint == null) return;

        Vector2 origin = attackPoint.position;
        Vector2 direction = TryGetComponent(out Player player) ? player.facingDirection.normalized : Vector2.down;
        float range = currentWeapon.attackRange;
        float halfArc = currentWeapon.attackArc / 2f;

        // Faint full circle for context: enemies here are found, but skipped if outside the slice.
        Gizmos.color = new Color(1f, 1f, 1f, 0.25f);
        Gizmos.DrawWireSphere(origin, range);

        Gizmos.color = Color.red;

        // Gizmos has no arc function, so build the curved edge from short straight lines:
        // walk from -halfArc to +halfArc and connect each point on the circle to the next.
        const int segments = 24;
        Vector2 previous = origin + RotateBy(direction, -halfArc) * range;
        Gizmos.DrawLine(origin, previous);

        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.Lerp(-halfArc, halfArc, (float)i / segments);
            Vector2 point = origin + RotateBy(direction, angle) * range;
            Gizmos.DrawLine(previous, point);
            previous = point;
        }

        Gizmos.DrawLine(previous, origin);
    }

    // Rotates a 2D vector around the Z axis (positive = counter-clockwise).
    private static Vector2 RotateBy(Vector2 vector, float degrees)
    {
        return Quaternion.Euler(0f, 0f, degrees) * vector;
    }
}
