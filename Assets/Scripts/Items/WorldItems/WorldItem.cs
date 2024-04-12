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

    protected int numItemsInStack;

    protected virtual void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        InitializeItemFromBaseItemData();
        if (!item.stackable)
        {
            numItemsInStack = 1;
        }
    }

    public float GetWeight()
    {
        return item.Weight * numItemsInStack;
    }

    protected virtual void InitializeItemFromBaseItemData()
    {
        
    }

    public void SetNumberOfStartingItems(int numItems)
    {
        numItemsInStack = numItems;
    }

    public int GetNumberOfItems()
    {
        return numItemsInStack;
    }

    public void ChangeNumberOfItems(int amount)
    {
        numItemsInStack += amount;
    }

    protected virtual void Start()
    {
        
    }

    public virtual void Use()
    {

    }

    public BaseItem GetBaseItem()
    {
        return item;
    }

    public void SetUninteractableTemporarily()
    {
        StartCoroutine(TempInteractionOff(timeDelay));
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
