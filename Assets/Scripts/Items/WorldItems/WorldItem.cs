using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WorldItem : MonoBehaviour, IInteractable
{
    [SerializeField] protected SharedItemData sharedItemData;

    Rigidbody rigidBody;

    [SerializeField] float timeDelay = 1.0f;
    bool interactable = true;

    [SerializeField] protected int numItemsInStack;

    public delegate void PickedUp();
    public event PickedUp OnPickedUp;

    bool isOpen = false;

    protected virtual void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        InitializeItemFromBaseItemData();
        if (!sharedItemData.Stackable || numItemsInStack <= 0)
        {
            numItemsInStack = 1;
        }
    }

    protected virtual void Start()
    {
        if (sharedItemData.ColorGameObjectBasedOnRarity)
        {
            GetComponentInChildren<Renderer>().material.color = RarityColorManager.Instance.GetColorByRarity(sharedItemData.Rarity);
        }
    }

    // Function to create an ItemInstance from this WorldItem
    public virtual ItemInstance CreateItemInstance()
    {
        ItemInstance instance = new ItemInstance(sharedItemData);
        instance.SetProperty(ItemAttributeKey.NumItemsInStack, numItemsInStack);
        return instance;
    }

    public virtual ItemInstance CreateNewItemInstance(SharedItemData sharedData)
    {
        ItemInstance instance = new ItemInstance(sharedData);
        instance.SetProperty(ItemAttributeKey.NumItemsInStack, 1);
        return instance;
    }

    public virtual void InitializeFromItemInstance(ItemInstance instance)
    {
        sharedItemData = instance.sharedData;
        numItemsInStack = (int)instance.GetProperty(ItemAttributeKey.NumItemsInStack);
    }

    public float GetWeight()
    {
        return sharedItemData.Weight * numItemsInStack;
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

    public SharedItemData GetSharedItemData()
    {
        return sharedItemData;
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

    public void Interact()
    {
        bool pickedUp = PlayerInventory.Instance.AddItem(this);
        if (pickedUp)
        {
            if (OnPickedUp != null)
            {
                OnPickedUp();
            }
            HideUI();
            Destroy(this.gameObject);
        }
    }

    public void ShowUI()
    {
        if (!isOpen)
        {
            isOpen = true;
            PickupItemMenu.Instance.Open();
        }
        PickupItemMenu.Instance.UpdatePickupPromptInfo(this);
    }

    public void HideUI()
    {
        if (isOpen)
        {
            PickupItemMenu.Instance.Close();
            isOpen = false;
        }
    }
}
