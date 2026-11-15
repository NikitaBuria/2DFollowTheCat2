using UnityEngine;

// Every enemy sound in one place (Wisps, minions, the King Wisp). Gameplay only fires events; this
// listens and plays the SoundData from the enemy's data asset (CharacterData hit/death, EnemyData
// attack/hover, BossData summon) through the mixer group set on each SoundData.
[RequireComponent(typeof(Enemy), typeof(Health), typeof(EnemyCombat))]
public class EnemySoundController : SoundPlayer
{
    [Tooltip("Looping hum on a child object. Its AudioSource keeps its own 3D settings, so it is only heard nearby.")]
    [SerializeField] private AudioSource hoverSource;

    private Enemy enemy;
    private Health health;
    private EnemyCombat enemyCombat;
    // Only bosses have one.
    private EnemySummoner enemySummoner;

    protected override void Awake()
    {
        base.Awake();
        enemy = GetComponent<Enemy>();
        health = GetComponent<Health>();
        enemyCombat = GetComponent<EnemyCombat>();
        TryGetComponent(out enemySummoner);
    }

    private void OnEnable()
    {
        health.OnDamaged += PlayHit;
        health.OnDied += PlayDeath;
        enemyCombat.OnAttack += PlayAttack;
        if (enemySummoner != null) enemySummoner.OnSummoned += PlaySummon;

        // Pooled enemies are re-enabled when they respawn, so the hum starts again.
        StartHover();
    }

    private void OnDisable()
    {
        health.OnDamaged -= PlayHit;
        health.OnDied -= PlayDeath;
        enemyCombat.OnAttack -= PlayAttack;
        if (enemySummoner != null) enemySummoner.OnSummoned -= PlaySummon;
    }

    private void PlayHit(DamageData damage) => Play(enemy.enemyData.hitSound);
    // Detached: the enemy is despawned before a long death sound would end.
    private void PlayDeath() => PlayDetached(enemy.enemyData.deathSound);
    private void PlayAttack() => Play(enemy.enemyData.attackSound);
    private void PlaySummon() => Play(enemy is KingWisp boss ? boss.bossData.summonSound : null);

    private void StartHover()
    {
        SoundData sound = enemy.enemyData.hoverSound;
        AudioClip clip = sound != null ? sound.PickClip() : null;
        if (hoverSource == null || clip == null) return;

        hoverSource.playOnAwake = false;
        hoverSource.loop = true;
        hoverSource.clip = clip;
        hoverSource.volume = sound.volumeScale;
        hoverSource.pitch = sound.PickPitch();
        if (sound.output != null) hoverSource.outputAudioMixerGroup = sound.output;
        hoverSource.Play();
    }
}
