// Anything that can be hit: player, enemies, breakable objects.
public interface IDamageable
{
    bool isAlive { get; }
    void TakeDamage(DamageData damage);
}
