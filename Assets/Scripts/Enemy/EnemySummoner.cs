using UnityEngine;
using UnityEngine.Pool;

// How a boss summons: spawns minions in a circle and recycles them through an ObjectPool, so a long
// fight doesn't create and destroy Wisps over and over. KingWispSummonState decides when.
// Stats come from the boss's BossData.
[RequireComponent(typeof(Enemy))]
public class EnemySummoner : MonoBehaviour
{
    private BossData bossData;
    private ObjectPool<Enemy> pool;
    private float nextSummonTime;

    public int aliveMinions => pool.CountActive;

    // Cooldown over and room for more minions.
    public bool canSummon =>
        bossData.minionPrefab != null && Time.time >= nextSummonTime && aliveMinions < bossData.maxMinions;

    private void Awake()
    {
        bossData = (BossData)GetComponent<Enemy>().enemyData;
        pool = new ObjectPool<Enemy>(
            CreateMinion,
            actionOnRelease: minion => minion.gameObject.SetActive(false),
            actionOnDestroy: minion => Destroy(minion.gameObject),
            defaultCapacity: bossData.maxMinions,
            maxSize: bossData.maxMinions);
    }

    // Sleeping minions die with the boss; awake ones finish their fight (see Release).
    private void OnDestroy()
    {
        pool.Clear();
    }

    // Spawns up to minionCount minions on a circle around the boss and starts the cooldown.
    public void Summon()
    {
        nextSummonTime = Time.time + bossData.summonCooldown;

        int count = Mathf.Min(bossData.minionCount, bossData.maxMinions - aliveMinions);
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * bossData.summonRadius;

            Enemy minion = pool.Get();
            minion.transform.position = (Vector2)transform.position + offset;
            minion.gameObject.SetActive(true);
            minion.ResetForSpawn();
        }
    }

    // Built asleep; Summon positions and wakes it.
    private Enemy CreateMinion()
    {
        Enemy minion = Instantiate(bossData.minionPrefab, transform.parent);
        minion.AssignToPool(Release);
        minion.gameObject.SetActive(false);
        return minion;
    }

    // A dead minion comes back here. If the boss is already gone, there's no pool to return to.
    private void Release(Enemy minion)
    {
        if (this == null)
        {
            Destroy(minion.gameObject);
            return;
        }

        pool.Release(minion);
    }
}
