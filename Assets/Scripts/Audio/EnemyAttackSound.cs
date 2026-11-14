using UnityEngine;

// Plays the enemy's EnemyData.attackSound at the moment of its hit. Listens to EnemyCombat.OnAttack.
[RequireComponent(typeof(Enemy), typeof(EnemyCombat))]
public class EnemyAttackSound : SoundPlayer
{
    private Enemy enemy;
    private EnemyCombat enemyCombat;

    protected override void Awake()
    {
        base.Awake();
        enemy = GetComponent<Enemy>();
        enemyCombat = GetComponent<EnemyCombat>();
    }

    private void OnEnable()
    {
        enemyCombat.OnAttack += PlayAttack;
    }

    private void OnDisable()
    {
        enemyCombat.OnAttack -= PlayAttack;
    }

    private void PlayAttack() => Play(enemy.enemyData.attackSound);
}
