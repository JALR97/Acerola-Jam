using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    //Balance
    [SerializeField] private int maxHealth = 100;
    
    //Private
    private int currentHealth;
    
    //Functions
    public void Damage(int dmg) {
        //play sounds
        currentHealth -= dmg;
        if (currentHealth <= 0) {
            Death();
        }
    }

    public void Heal(int hp) {
        //Sounds
        currentHealth = Mathf.Min(currentHealth + hp, maxHealth);
    }
    
    private void Death() {
        //died
        Debug.Log("Death");
        Destroy(gameObject);
    }
    
}
