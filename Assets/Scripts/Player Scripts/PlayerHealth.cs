using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    private readonly int health =100 ;
    public event Action playerDied;

    public bool isDead;



    public bool IsAlive ()
    {

        if (health <= 0)
        {
          isDead = true;
          playerDied?.Invoke();
        } 

        return isDead;
    }

    private void OnEnable()
    {
        
        
    }







}
