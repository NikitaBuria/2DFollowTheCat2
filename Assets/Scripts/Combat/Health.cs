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

    // Optional (e.g. the player's Equipment for armor); null = full damage.
    private IDamageModifier damageModifier;
    public bool isInvincible => Time.time < invincibleUntil;

    // Fired on every hit that lands, before OnDied, so listeners can read the hit (knockback, direction).
    public event Action<DamageData> OnDamaged;
    public event Action OnDied;
    // Sends (current, max) whenever currentHealth changes; the health bar listens to it.
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        TryGetComponent(out damageModifier);
    }

    public void Initialize(int startingMaxHealth)
    {
        maxHealth = Mathf.Max(1, startingMaxHealth);
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // The owner's states decide when (e.g. Hurt, later Dash); Health only knows how.
    public void StartInvincibility(float duration)
    {
        // Never shortens i-frames that are already running (a dash right after a hit).
        invincibleUntil = Mathf.Max(invincibleUntil, Time.time + duration);
    }

    // Returns true when health went up (false when dead or already full).
    public bool Heal(int amount)
    {
        if (!isAlive || currentHealth >= maxHealth || amount <= 0) return false;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        return true;
    }

    public void Save(SaveData data) => data.health = currentHealth;

    // Saves are only made while alive, so 0 is invalid too.
    public string Validate(SaveData data) =>
        data.health < 1 || data.health > maxHealth ? $"health {data.health} is outside 1-{maxHealth}" : null;

    public void Load(SaveData data)
    {
        currentHealth = data.health;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(DamageData damage)
    {
        if (!isAlive || isInvincible) return;

        // Listeners get the hit as it was actually taken (after armor).
        if (damageModifier != null)
            damage = new DamageData(damageModifier.ModifyIncomingDamage(damage.amount),
                damage.direction, damage.knockback, damage.knockbackDuration);

        currentHealth = Mathf.Max(0, currentHealth - damage.amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamaged?.Invoke(damage);

        if (!isAlive)
            OnDied?.Invoke();
    }
}
