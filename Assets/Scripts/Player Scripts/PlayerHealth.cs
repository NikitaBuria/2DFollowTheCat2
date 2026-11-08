using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    private int health;

    public bool isAlive => health > 0;
    public Transform damageTransform => transform;

    public event Action OnDied;
    public event Action<DamageData> OnHurt;

    // Called by Player with the max health from its CharacterData.
    public void Initialize(int maxHealth)
    {
        health = maxHealth;
    }

    public void TakeDamage(DamageData damage)
    {
        if (!isAlive) return;

        health -= damage.amount;
        Debug.Log($"Player took {damage.amount} damage. Health is now {health}.");

        if (health <= 0)
        {
            health = 0;
            Debug.Log("Player has died.");
            OnDied?.Invoke();
            return;
        }

        Debug.Log("Player is hurt.");
        OnHurt?.Invoke(damage);
    }
}
