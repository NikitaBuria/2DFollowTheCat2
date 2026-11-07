using System;
using UnityEngine;

// Draft: will be replaced by a shared Health component in the Health/Hurt/Dead step.
public class PlayerHealth : MonoBehaviour
{
    private readonly int health = 100;

    public event Action OnDied;

    public bool isDead { get; private set; }

    public bool IsAlive()
    {
        if (health <= 0)
        {
            isDead = true;
            OnDied?.Invoke();
        }

        return isDead;
    }
}
