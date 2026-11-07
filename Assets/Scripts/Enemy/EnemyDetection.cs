using System.Collections.Generic;
using UnityEngine;

// Finds the closest target the enemy can see. States only read `target`; Enemy calls Refresh every frame.
// Searching is throttled (targetRefreshInterval); losing a target is checked every frame.
public class EnemyDetection : MonoBehaviour
{
    private EnemyData enemyData;
    private ContactFilter2D targetFilter;
    private float nextSearchTime;

    // Reused for every search so OverlapCircle doesn't create a new array each time.
    private readonly List<Collider2D> hits = new();

    public Transform target { get; private set; }

    public void Initialize(EnemyData data)
    {
        enemyData = data;
        targetFilter = new ContactFilter2D { useTriggers = true };
        targetFilter.SetLayerMask(enemyData.targetLayers);
        Clear();
    }

    public void Clear()
    {
        target = null;
        nextSearchTime = 0f;
    }

    public void Refresh()
    {
        if (enemyData == null) return;

        // Losing a target is cheap, so it happens right away.
        if (target != null && !IsStillValid(target))
            target = null;

        if (target != null || Time.time < nextSearchTime) return;

        nextSearchTime = Time.time + enemyData.targetRefreshInterval;
        target = FindClosestVisibleTarget();
    }

    private Transform FindClosestVisibleTarget()
    {
        Vector2 origin = transform.position;
        Physics2D.OverlapCircle(origin, enemyData.detectionRange, targetFilter, hits);

        Transform closest = null;
        float closestDistance = float.PositiveInfinity;

        foreach (Collider2D hit in hits)
        {
            // The Rigidbody's object is the character's root, even when the collider sits on a child.
            Transform candidate = hit.attachedRigidbody != null ? hit.attachedRigidbody.transform : hit.transform;
            if (!IsAlive(candidate)) continue;

            float distance = Vector2.Distance(origin, candidate.position);
            if (distance >= closestDistance || !CanSee(candidate.position)) continue;

            closestDistance = distance;
            closest = candidate;
        }

        return closest;
    }

    // Line of sight is only needed to spot a target. Once chasing, the enemy keeps chasing
    // around corners until the target is out of loseTargetRange or dead.
    private bool IsStillValid(Transform candidate)
    {
        return candidate.gameObject.activeInHierarchy
            && IsAlive(candidate)
            && Vector2.Distance(transform.position, candidate.position) <= enemyData.loseTargetRange;
    }

    // Targets without health (no IDamageable yet) count as alive.
    private static bool IsAlive(Transform candidate)
    {
        return !candidate.TryGetComponent(out IDamageable damageable) || damageable.isAlive;
    }

    private bool CanSee(Vector2 point)
    {
        if (enemyData.obstacleLayers == 0) return true;

        // Linecast returns the first wall between the enemy and the point; no wall = visible.
        return !Physics2D.Linecast(transform.position, point, enemyData.obstacleLayers);
    }
}
