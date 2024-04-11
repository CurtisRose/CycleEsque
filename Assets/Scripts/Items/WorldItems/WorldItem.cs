using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour
{
    [SerializeField] BaseItem item;
    Rigidbody rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    public BaseItem GetBaseItem()
    {
        return item;
    }

    public void Equip()
    {
        if (rigidBody != null)
        {
            rigidBody.isKinematic = true;
        }
    }
}
