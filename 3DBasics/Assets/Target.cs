using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField, Range(10, 300)] private float maxHealth;
    private Health targetHealth;

    private void Awake()
    {
        
        targetHealth = new Health(maxHealth);
       
    }

    public void TakeDamage(float dmgAmount)
    {
        targetHealth.CurrHealth -= dmgAmount;
       
        if(targetHealth.CurrHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    public float getCurrentHealth()
    {
        return targetHealth.CurrHealth;
    }
}
