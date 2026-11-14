using UnityEngine;

// The King Wisp boss: an ordinary Enemy that also summons Wisps.
// KingWispSummonState decides when, EnemySummoner does the summoning. Its data must be a BossData.
[RequireComponent(typeof(EnemySummoner))]
public class KingWisp : Enemy
{
    public EnemySummoner enemySummoner { get; private set; }
    public BossData bossData => (BossData)enemyData;
    public KingWispSummonState summonState { get; private set; }

    protected override void GetReferences()
    {
        base.GetReferences();
        enemySummoner = GetComponent<EnemySummoner>();
    }

    protected override void CreateStates()
    {
        base.CreateStates();
        summonState = new KingWispSummonState(this);
    }

    // Chase asks this before attacking.
    public override EnemyState GetSpecialState() => enemySummoner.canSummon ? summonState : null;
}
