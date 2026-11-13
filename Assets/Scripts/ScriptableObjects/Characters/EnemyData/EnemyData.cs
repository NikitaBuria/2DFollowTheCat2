using UnityEngine;

// Read-only stats for one enemy type. Inherits health and speeds from CharacterData:
// walkSpeed is used for patrolling, runSpeed for chasing.
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Scriptable Objects/Enemy Data")]
public class EnemyData : CharacterData
{
    [Header("Detection")]
    // Layers the enemy hunts (the player).
    [field: SerializeField] public LayerMask targetLayers { get; private set; }
    // Layers that block sight (walls). Nothing = the enemy sees through everything.
    [field: SerializeField] public LayerMask obstacleLayers { get; private set; }
    [field: SerializeField, Min(0f)] public float detectionRange { get; private set; } = 5f;
    // Bigger than detectionRange, so a target at the edge doesn't flip between found and lost.
    [field: SerializeField, Min(0f)] public float loseTargetRange { get; private set; } = 7f;
    // Seconds between searches for a new target. Searching every frame is wasted work.
    [field: SerializeField, Min(0.05f)] public float targetRefreshInterval { get; private set; } = 0.2f;

    [Header("Patrol")]
    // Seconds the enemy waits at each patrol point.
    [field: SerializeField, Min(0f)] public float patrolWaitTime { get; private set; } = 1f;
    // How close counts as "arrived" at a patrol point.
    [field: SerializeField, Min(0.01f)] public float arriveDistance { get; private set; } = 0.1f;

    [Header("Attack (body / claws, no weapon)")]
    [field: SerializeField, Min(0)] public int attackDamage { get; private set; } = 10;
    // Reach, measured to the closest point of the target's collider.
    [field: SerializeField, Min(0.1f)] public float attackRange { get; private set; } = 0.8f;
    [field: SerializeField, Min(0f)] public float knockback { get; private set; } = 4f;
    [field: SerializeField, Min(0f)] public float knockbackDuration { get; private set; } = 0.15f;
    // Telegraph: how long the enemy stands still before the hit, so the player can react.
    [field: SerializeField, Min(0f)] public float windupTime { get; private set; } = 0.4f;
    // How long the enemy stays in the attack after the hit before acting again.
    [field: SerializeField, Min(0f)] public float recoverTime { get; private set; } = 0.6f;

    [Header("Dead")]
    // Seconds before the dead enemy is removed (time for the death animation).
    [field: SerializeField, Min(0f)] public float destroyDelay { get; private set; } = 1f;

    [Header("Loot")]
    // Dropped once on death. Empty = drops nothing.
    [field: SerializeField] public LootTable lootTable { get; private set; }

    // Runs in the Editor when a value changes: keeps the lose range at least as big as the detection range.
    private void OnValidate()
    {
        if (loseTargetRange < detectionRange)
            loseTargetRange = detectionRange;
    }
}
