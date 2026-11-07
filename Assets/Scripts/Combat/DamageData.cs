using UnityEngine;

// Describes one hit so every damageable object receives the same information.
public readonly struct DamageData
{
    public int amount { get; }
    public Vector2 direction { get; }
    public float knockback { get; }
    public float knockbackDuration { get; }

    // The parameters have the same names as the properties, so "this." picks the property.
    public DamageData(int amount, Vector2 direction, float knockback, float knockbackDuration)
    {
        this.amount = Mathf.Max(0, amount);
        this.direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        this.knockback = Mathf.Max(0f, knockback);
        this.knockbackDuration = Mathf.Max(0f, knockbackDuration);
    }
}
