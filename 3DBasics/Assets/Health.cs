using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health 
{
    private float curHealth;
    private float maxHealth;
    public float CurrHealth {
        get
        {
            return curHealth;
        } 
        set
        {
            if(value > maxHealth)
            {
                curHealth = maxHealth;
            }

            if(value <= 0)
            {
                curHealth = 0;
            }

            curHealth = value;
        } 
    }
    public float MaxHealth { 
        get { return maxHealth; }
        set 
        {
            maxHealth = value;
        }
    }

    public Health(float currHealth, float MaxHealth)
    {
        this.MaxHealth = MaxHealth;
        this.CurrHealth = currHealth;

    }

    public Health(float MaxHealth)
    {
       this.MaxHealth = MaxHealth;
       curHealth = MaxHealth;
       
    }


}
