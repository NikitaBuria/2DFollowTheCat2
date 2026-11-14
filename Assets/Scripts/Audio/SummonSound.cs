using UnityEngine;

// Plays the boss's BossData.summonSound when minions appear. Listens to EnemySummoner.OnSummoned.
[RequireComponent(typeof(Enemy), typeof(EnemySummoner))]
public class SummonSound : SoundPlayer
{
    private Enemy enemy;
    private EnemySummoner enemySummoner;

    protected override void Awake()
    {
        base.Awake();
        enemy = GetComponent<Enemy>();
        enemySummoner = GetComponent<EnemySummoner>();
    }

    private void OnEnable()
    {
        enemySummoner.OnSummoned += PlaySummon;
    }

    private void OnDisable()
    {
        enemySummoner.OnSummoned -= PlaySummon;
    }

    private void PlaySummon() => Play(enemy.enemyData is BossData boss ? boss.summonSound : null);
}
