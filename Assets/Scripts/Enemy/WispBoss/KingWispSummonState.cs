using UnityEngine;

// The King Wisp stands still and plays Spawn; the minions appear at summonDelay.
// -> Chase (or Idle without a target) when the clip ends. A hit before summonDelay cancels it.
public class KingWispSummonState : EnemyState
{
    private readonly KingWisp kingWisp;
    private float timer;
    private bool hasSummoned;

    public KingWispSummonState(KingWisp kingWisp) : base(kingWisp)
    {
        this.kingWisp = kingWisp;
    }

    public override void Enter()
    {
        timer = 0f;
        hasSummoned = false;
        enemy.enemyMotor.Stop();
        enemy.enemyAnimator.SetAnimationState(EnemyAnimationState.Spawn);
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (!hasSummoned && timer >= kingWisp.bossData.summonDelay)
        {
            hasSummoned = true;
            kingWisp.Summon();
        }

        if (timer < kingWisp.bossData.summonDuration) return;

        if (enemy.target != null)
            enemy.enemyStateMachine.ChangeState(enemy.chaseState);
        else
            enemy.enemyStateMachine.ChangeState(enemy.idleState);
    }

    public override void FixedUpdate()
    {
        enemy.enemyMotor.Stop();
    }

    public override void Exit()
    {
        enemy.enemyMotor.Stop();
    }
}
