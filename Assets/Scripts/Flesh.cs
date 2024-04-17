using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flesh : MonoBehaviour
{
    [SerializeField] Color successfulHitColor;
    [SerializeField] Renderer targetRenderer;

    [SerializeField] float health;

    [SerializeField] FlickerMaterial flickerMaterial;

    // Is this flesh being killed critical to the relevant entity?
    [SerializeField] bool criticalFlesh;

    public delegate void Destroyed(Flesh flesh);
    public event Destroyed OnFleshDestroyed;

    public void ReceiveDamage(float damage, float armorPenetration)
    {
        // For now, just stop taking damage.
        // Later, maybe this does damage to the overall entity or something.
        if (health <= 0)
        {
            return;
        }

        if (flickerMaterial != null)
        {
            flickerMaterial.Flicker(successfulHitColor);
        }

        health -= damage;

        if (health <= 0)
        {
            Destroy(flickerMaterial);
            targetRenderer.material.color = successfulHitColor;
            
            if (OnFleshDestroyed != null)
            {
                OnFleshDestroyed(this);
            }
        }
    }

    public bool IsCriticalFlesh()
    {
        return criticalFlesh;
    }
}
