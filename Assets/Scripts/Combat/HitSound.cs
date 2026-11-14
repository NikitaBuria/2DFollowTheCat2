using UnityEngine;

// Plays the owner's CharacterData.hitSound when it takes a hit. Only listens to Health.OnDamaged,
// so combat code never knows about audio.
[RequireComponent(typeof(Health))]
public class HitSound : SoundPlayer
{
    private Health health;
    private CharacterData data;

    protected override void Awake()
    {
        base.Awake();
        health = GetComponent<Health>();
        data = GetCharacterData();
    }

    private void OnEnable()
    {
        health.OnDamaged += PlayHit;
    }

    private void OnDisable()
    {
        health.OnDamaged -= PlayHit;
    }

    private void PlayHit(DamageData damage) => Play(data != null ? data.hitSound : null);
}
