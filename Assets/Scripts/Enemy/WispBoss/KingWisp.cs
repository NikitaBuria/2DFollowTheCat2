using System.Collections.Generic;
using UnityEngine;

// The King Wisp boss: an ordinary Enemy that also summons Wisps (KingWispSummonState).
// Its data must be a BossData.
public class KingWisp : Enemy
{
    public BossData bossData => (BossData)enemyData;
    public KingWispSummonState summonState { get; private set; }

    // Minions summoned by this boss; destroyed ones turn into null and are removed.
    private readonly List<Enemy> minions = new List<Enemy>();
    private float nextSummonTime;

    protected override void CreateStates()
    {
        base.CreateStates();
        summonState = new KingWispSummonState(this);
    }

    // Chase asks this: summon when the cooldown is over and there's room for more minions.
    public override EnemyState GetSpecialState()
    {
        if (bossData.minionPrefab == null || Time.time < nextSummonTime) return null;

        minions.RemoveAll(minion => minion == null);
        if (minions.Count >= bossData.maxMinions) return null;

        return summonState;
    }

    // Spawns the minions in a circle around the boss and starts the cooldown.
    public void Summon()
    {
        nextSummonTime = Time.time + bossData.summonCooldown;

        int count = Mathf.Min(bossData.minionCount, bossData.maxMinions - minions.Count);
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * bossData.summonRadius;
            Enemy minion = Instantiate(bossData.minionPrefab, (Vector2)transform.position + offset, Quaternion.identity, transform.parent);
            minions.Add(minion);
        }
    }
}
