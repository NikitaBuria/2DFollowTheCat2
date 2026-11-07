using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{


    [SerializeField] private int maxHealth = 100;
    private int health;
    public bool IsAlive => health > 0;

    public Transform DamageTransform => transform;



    private void Awake()
    {
        health = maxHealth;
    }

    public void Die()
    {
        
    }

    public void TakeDamage(DamageData damage)
    {
        health -= damage.Amount;
        Debug.Log($"{name} took {damage.Amount} damage. Health is now {health}.");
        if (health <= 0)
        {
            Die();
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
