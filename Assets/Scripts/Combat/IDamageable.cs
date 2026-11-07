using UnityEngine;

public interface IDamageable
{
    bool IsAlive { get; }
    Transform DamageTransform { get; }
    void TakeDamage(DamageData damage);


    
}
