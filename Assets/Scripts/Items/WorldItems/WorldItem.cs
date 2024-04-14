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

    protected virtual void Start()
    {
        if (item.ColorGameObjectBasedOnRarity)
        {
            GetComponentInChildren<Renderer>().material.color = RarityColorManager.Instance.GetColorByRarity(item.Rarity);
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

    public virtual bool Use()
    {
        return true;
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
        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Player"));
    }

    public virtual void Unequip()
    {
        if (rigidBody != null)
        {
            rigidBody.isKinematic = false;
        }
        interactable = true;
        SetLayerRecursively(gameObject, LayerMask.NameToLayer("WorldItems"));
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

    public void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void UpdateBaseItemData(BaseItem item)
    {
        this.item = item;
    }
}
