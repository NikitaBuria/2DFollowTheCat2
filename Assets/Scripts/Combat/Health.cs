using System;
using UnityEngine;

// Shared health for anything that can be hit (player and enemies).
// The owner sets max health from its data; listeners react through the events.
// Saved only on the player (SaveManager collects the player's ISaveables); enemies ignore it.
public class Health : MonoBehaviour, IDamageable, ISaveable
{
    public int maxHealth { get; private set; }
    public int currentHealth { get; private set; }

    public bool isAlive => currentHealth > 0;

    // Damage is ignored until this time. A timestamp instead of a countdown, so no Update is needed.
    private float invincibleUntil;
    public bool isInvincible => Time.time < invincibleUntil;

    // Fired on every hit that lands, before OnDied, so listeners can read the hit (knockback, direction).
    public event Action<DamageData> OnDamaged;
    public event Action OnDied;

    public void Initialize(int startingMaxHealth)
    {
        maxHealth = Mathf.Max(1, startingMaxHealth);
        currentHealth = maxHealth;
    }

    // The owner's states decide when (e.g. Hurt, later Dash); Health only knows how.
    public void StartInvincibility(float duration)
    {
        invincibleUntil = Time.time + duration;
    }

    // Returns true when health went up (false when dead or already full).
    public bool Heal(int amount)
    {
        if (!isAlive || currentHealth >= maxHealth || amount <= 0) return false;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        return true;
    }

    public void Save(SaveData data) => data.health = currentHealth;

    // Saves are only made while alive, so 0 is invalid too.
    public string Validate(SaveData data) =>
        data.health < 1 || data.health > maxHealth ? $"health {data.health} is outside 1-{maxHealth}" : null;

    public void Load(SaveData data) => currentHealth = data.health;

    public void TakeDamage(DamageData damage)
    {
        if (!isAlive || isInvincible) return;

        currentHealth = Mathf.Max(0, currentHealth - damage.amount);
        OnDamaged?.Invoke(damage);

        if (!isAlive)
            OnDied?.Invoke();
    }
}
