using UnityEngine;

// Plays the owner's CharacterData.deathSound (player, Wisps, King Wisp). Listens to Health.OnDied.
// Detached, because enemies are despawned before a long death sound would end.
[RequireComponent(typeof(Health))]
public class DeathSound : SoundPlayer
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
        health.OnDied += PlayDeath;
    }

    private void OnDisable()
    {
        health.OnDied -= PlayDeath;
    }

    private void PlayDeath() => PlayDetached(data != null ? data.deathSound : null);
}
