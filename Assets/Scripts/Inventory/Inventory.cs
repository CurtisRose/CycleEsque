using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Inventory : MonoBehaviour
{
    public BaseItem[] startItems;
    public List<InventorySlot> inventorySlots;
    public GameObject inventoryItemPrefab;
    [SerializeField] private float inventoryWeightLimit;
    public float currentWeight;
    [SerializeField] public static List<Color> rarityColors;

    protected void Start()
    {
        foreach (BaseItem startItem in startItems)
        {
            AddItem(startItem);
        }
    }

    public virtual float GetInventoryWeightLimit()
    {
        return inventoryWeightLimit;
    }

    public bool AddItem(WorldItem item, int numItems = 1)
    {
        bool partialOnly = false;
        if (item.GetWeight() > GetInventoryWeightLimit() - currentWeight)
        {
            if (item.GetBaseItem().stackable)
            {
                float weightToFillBackpack = GetInventoryWeightLimit() - currentWeight;
                numItems = (int)Mathf.Floor(weightToFillBackpack / item.GetBaseItem().Weight);
                partialOnly = true;
                if (numItems == 0)
                {
                    return false;
                }
            } else
            {
                return false;
            }
        }

        BaseItem itemPickedUp = item.GetBaseItem();
        bool successCheck = AddItem(itemPickedUp, numItems);

        if (partialOnly)
        {
            item.ChangeNumberOfItems(-numItems);
            return false;
        }

        return successCheck && !partialOnly;
    }

    public bool AddItem(BaseItem item, int numItems = 1)
    {
        float temp = GetInventoryWeightLimit();
        if (item.Weight * numItems > GetInventoryWeightLimit() - currentWeight)
        {
            return false;
        }

        // Check if any slot already has item with count lower than the stack size
        if (item.stackable)
        {
            InventorySlot firstEmptySlot = null;
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                InventorySlot slot = inventorySlots[i];
                InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
                if (itemInSlot != null &&
                    itemInSlot.item == item &&
                    itemInSlot.GetItemCount() < itemInSlot.item.maxStackSize)
                {
                    itemInSlot.ChangeItemCount(numItems);
                    UpdateWeight(item.Weight * numItems);
                    return true;
                }
                if (!slot.HasItem() && firstEmptySlot == null)
                {
                    firstEmptySlot = slot;
                }
            }
            // No items of same type were found, Add it to the earliest empty slot
            if (firstEmptySlot != null)
            {
                CreateNewItem(item, firstEmptySlot, numItems);
                return true;
            }
        }

        // Find an empty slot
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            // If slot is empty
            if (itemInSlot == null)
            {
                CreateNewItem(item, slot);
                return true;
            }
        }

        // No Empty Slots
        // TODO: Create More Slots Dynamically
        // Inventory is only limited by weight

        Debug.Log("Inventory Is Full");
        return false;
    }

    public int GetNumberOfItemsOfType(ItemType type)
    {
        int numItems = 0;
        foreach(InventorySlot inventorySlot in inventorySlots)
        {
            InventoryItem itemInSlot = inventorySlot.GetItemInSlot();
            if (itemInSlot != null && inventorySlot.GetItemInSlot().GetItemType() == type)
            {
                if (inventorySlot.GetItemInSlot().item.stackable)
                {
                    numItems += inventorySlot.GetItemInSlot().GetItemCount();
                } else
                {
                    numItems++;
                }
            }
        }
        return numItems;
    }

    public virtual void UpdateWeight(float amount)
    {
        currentWeight += amount;
    }

    // TODO:
    public bool RemoveItem(BaseItem item)
    {
        Debug.Log("This Function has NOT Been Implemented");
        return true;
    }

    protected void CreateNewItem(BaseItem item, InventorySlot inventorySlot, int numberOfItems = 1)
    {
        GameObject newItem = Instantiate(inventoryItemPrefab, inventorySlot.itemSlot);
        InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
        inventoryItem.name = item.DisplayName;
        inventoryItem.InitializeItem(item);
        inventoryItem.ChangeItemCount(numberOfItems);
        inventorySlot.SetItemInSlotAfterDrag(inventoryItem);
    }

    public virtual void StartInventoryItemMoved(InventoryItem inventoryItem)
    {

    }

    public virtual void EndInventoryItemMoved(InventoryItem inventoryItem)
    {

    }

    public void AddItemToEarliestEmptySlot(InventoryItem inventoryItem)
    {
        InventorySlot emptySlot = null;
        foreach (InventorySlot slot in inventorySlots)
        {
            // If the earliest slot is it's own slot, then break.
            if (slot == inventoryItem.GetCurrentInventorySlot())
            {
                break;
            }
            if (!slot.HasItem())
            {
                emptySlot = slot;
                break;
            }
        }
        // If an earlier empty slot was found, then swap them
        if (emptySlot != null)
        {
            emptySlot.Swap(inventoryItem);
        }
    }

    public void PlaceItem(InventoryItem item, InventorySlot inventorySlot)
    {
        if (item != null)
        {
            inventorySlot.Swap(item);
        }
    }

    public virtual void QuickEquip(InventorySlot inventorySlot)
    {
        // Possibly, this can be used to right click items into earlier slots.
        // For now only used in the player inventory class
    }

    // For Testing
    protected void RemoveAllItemsFromEachSlot()
    {
        foreach (InventorySlot inventorySlot in inventorySlots)
        {
            if (inventorySlot.GetItemInSlot() != null)
            {
                InventoryItem item = inventorySlot.GetItemInSlot();
                inventorySlot.RemoveItemFromSlot();
                Destroy(item.gameObject);
            }
        }
    }
}
