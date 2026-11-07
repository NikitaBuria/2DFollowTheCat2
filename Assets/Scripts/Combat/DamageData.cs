using UnityEngine;

// Describes one hit so every damageable object receives the same information.
public readonly struct DamageData
{
    public DamageData(int amount, Vector2 direction, float knockback, float knockbackDuration)
    {
        Amount = Mathf.Max(0, amount);
        Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        Knockback = Mathf.Max(0f, knockback);
        KnockbackDuration = Mathf.Max(0f, knockbackDuration);
    }

    public int Amount { get; }
    public Vector2 Direction { get; }
    public float Knockback { get; }
    public float KnockbackDuration { get; }
}
