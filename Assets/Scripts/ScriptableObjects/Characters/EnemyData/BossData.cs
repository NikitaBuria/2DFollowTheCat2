using UnityEngine;

// Extra stats for a boss. Everything an ordinary enemy has comes from EnemyData.
[CreateAssetMenu(fileName = "NewBossData", menuName = "Scriptable Objects/Boss Data")]
public class BossData : EnemyData
{
    [Header("Summon")]
    // The enemy that is summoned (a prefab with an Enemy on it, using data without loot).
    [field: SerializeField] public Enemy minionPrefab { get; private set; }
    // Minions per summon.
    [field: SerializeField, Min(1)] public int minionCount { get; private set; } = 3;
    // No new summon while this many minions are alive.
    [field: SerializeField, Min(1)] public int maxMinions { get; private set; } = 6;
    // Seconds between summons.
    [field: SerializeField, Min(0f)] public float summonCooldown { get; private set; } = 8f;
    // Minions appear on a circle of this radius around the boss.
    [field: SerializeField, Min(0f)] public float summonRadius { get; private set; } = 1.5f;
    // When the minions appear in the Spawn clip (Astra's art: 0.2 s).
    [field: SerializeField, Min(0f)] public float summonDelay { get; private set; } = 0.2f;
    // Length of the whole summon = the Spawn clip (0.4 s).
    [field: SerializeField, Min(0f)] public float summonDuration { get; private set; } = 0.4f;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (summonDuration < summonDelay)
            summonDuration = summonDelay;
    }
}
