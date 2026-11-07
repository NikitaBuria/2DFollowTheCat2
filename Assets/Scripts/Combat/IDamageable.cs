using UnityEngine;

// Anything that can be hit: player, enemies, breakable objects.
public interface IDamageable
{
    bool isAlive { get; }
    Transform damageTransform { get; }
    void TakeDamage(DamageData damage);
}
