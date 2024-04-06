using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    List<Flesh> limbs;
    [SerializeField] int numLimbsToKill;
    int numLimbsDestroyed;
    bool isDead;

    private void Awake()
    {
        limbs = new List<Flesh>(GetComponentsInChildren<Flesh>());

        foreach (Flesh limbFlesh in limbs)
        {
            limbFlesh.OnFleshDestroyed += FleshDestroyed;
        }
    }

    private void FleshDestroyed(Flesh limb)
    {
        if (isDead)
        {
            Debug.Log("This Entity has already been killed, why have you destroyed another limb?");
            return;
        }
        Debug.Log("This Entity has lost a limb " + limb.name);
        numLimbsDestroyed++;
        if (limb.IsCriticalFlesh())
        {
            Debug.Log(limb.name + " is a critical limb, entity has been killed");
            isDead = true;
        }
        if (!isDead && numLimbsDestroyed >= numLimbsToKill)
        {
            Debug.Log("This Entity has lost too many limbs, entity has been killed");
            isDead = true;
        }
    }
}
