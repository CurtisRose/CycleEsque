using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armor : MonoBehaviour, IDamageable
{
    [SerializeField] float armorRating;
    [SerializeField] float armorHealth;
    [SerializeField] Flesh flesh;
    [SerializeField] FlickerMaterial flickerMaterial;
    [SerializeField] Color successfulHitColor;
    [SerializeField] Color failedHitColor;

    public void TakeDamage(float damage, float armorPenetration)
    {
        if (armorPenetration >= armorRating)
        {
            flickerMaterial.Flicker(successfulHitColor);
            // Armor has been penetrated
            armorHealth -= damage;

            if (armorHealth <= 0)
            {
                Destroy(gameObject);
                flesh.TakeDamage(damage, armorPenetration);
            }
            else
            {
                // Basically, the more the armor pen is than the armor rating, the more damage you will do to the flesh
                float armorPenRatio = ((armorPenetration - armorRating) / armorPenetration);
                flesh.TakeDamage(armorPenRatio * damage, armorPenetration);
            }
        }
        else
        {
            flickerMaterial.Flicker(failedHitColor);
        }
    }
}
