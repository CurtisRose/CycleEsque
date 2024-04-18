using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armor : MonoBehaviour
{
    [SerializeField] float armorRating;
    [SerializeField] float armorHealth;
    [SerializeField] Flesh flesh;
    [SerializeField] FlickerMaterial flickerMaterial;
    [SerializeField] Color successfulHitColor;
    [SerializeField] Color failedHitColor;

    public void ReceiveDamage(float damage, float armorPenetration)
    {
        if (armorPenetration >= armorRating)
        {
            flickerMaterial.Flicker(successfulHitColor);
            // Armor has been penetrated
            armorHealth -= damage;

            if (armorHealth <= 0)
            {
                Destroy(gameObject);
                flesh.ReceiveDamage(damage, armorPenetration);
            }
            else
            {
                // Basically, the more the armor pen is than the armor rating, the more damage you will do to the flesh
                float armorPenRatio = ((armorPenetration - armorRating) / armorPenetration);
                flesh.ReceiveDamage(armorPenRatio * damage, armorPenetration);
            }
        }
        else
        {
            flickerMaterial.Flicker(failedHitColor);
        }
    }
}
