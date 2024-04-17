using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour , IDamageable
{
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    private float healthBarVisibleTime = 5.0f;
    private float lastDamageTime;

    public delegate void HealthChanged(float currentHealth);
    public event HealthChanged OnHealthChanged;

    public delegate void DamageTaken(float damageAmount, float currentHealth);
    public event DamageTaken OnDamageTaken;

    public delegate void Healed(float damageHealed, float currentHealth);
    public event Healed OnHealed;

    public delegate void Death();
    public event Death OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
        lastDamageTime = -healthBarVisibleTime;
    }

    public void ReceiveDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        lastDamageTime = Time.time;

        OnHealthChanged?.Invoke(currentHealth);
        OnDamageTaken?.Invoke(amount, currentHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            //Debug.Log(gameObject.name + " has died.");
        }
    }

    public void ReceiveHealing(float amount)
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth);
            OnHealed?.Invoke(amount, currentHealth);
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool ShouldDisplayHealthBar()
    {
        return Time.time - lastDamageTime <= healthBarVisibleTime;
    }

    public float GetVisibilityTime()
    {
        return healthBarVisibleTime;
    }

    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        currentHealth = health;
    }
}