using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    private float currentHealth;

    public delegate void HealthChanged(float currentHealth);
    public event HealthChanged OnHealthChanged;

    public delegate void DamageTaken(float currentHealth);
    public event DamageTaken OnDamageTaken;

    public delegate void Death();
    public event Death OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, float penetration)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth);
        OnDamageTaken?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            Debug.Log(gameObject.name + " has died.");
        }
    }

    public void Heal(float amount)
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth);
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}