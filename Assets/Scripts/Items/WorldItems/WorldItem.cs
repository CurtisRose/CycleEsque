using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WorldItem : MonoBehaviour
{
    [SerializeField] protected BaseItem item;
    Rigidbody rigidBody;

    [SerializeField] float timeDelay = 1.0f;
    bool interactable = true;

    protected virtual void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        InitializeItem();
    }

    protected virtual void InitializeItem()
    {

    }

    protected virtual void Start()
    {
        StartCoroutine(TempInteractionOff(timeDelay));
    }

    public virtual void Use()
    {

    }

    public BaseItem GetBaseItem()
    {
        return item;
    }

    public virtual void Equip()
    {
        if (rigidBody != null)
        {
            rigidBody.isKinematic = true;
        }
        interactable = false;
    }

    public virtual void Unequip()
    {
        if (rigidBody != null)
        {
            rigidBody.isKinematic = false;
        }
        interactable = true;
    }

    IEnumerator TempInteractionOff(float delay)
    {
        interactable = false;
        yield return new WaitForSeconds(delay);
        interactable = true;
    }

    public bool IsInteractable()
    {
        return interactable;
    }
}
