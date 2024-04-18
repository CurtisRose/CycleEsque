using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Inventory : MonoBehaviour
{
    public SharedItemData[] startItems;
    public List<InventorySlot> inventorySlots;
    public GameObject inventoryItemPrefab;
    [SerializeField] private float inventoryWeightLimit;
    public float currentWeight;
    [SerializeField] public static List<Color> rarityColors;

    public delegate void InventoryChanged();
    public event InventoryChanged OnInventoryChanged;

    public delegate void ItemDropped(ItemInstance itemInstance);
    public event ItemDropped OnItemDropped;

    protected void Start()
    {
        foreach (SharedItemData startItem in startItems)
        {
            //ItemInstance itemInstance = new ItemInstance(startItem);
            WorldItem testItem = PlayerItemSpawner.Instance.GetPrefab(startItem);
            ItemInstance testInstance = testItem.CreateNewItemInstance(startItem);
  
            //itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, 1);
            AddItem(testInstance);
        }
    }

    public virtual float GetInventoryWeightLimit()
    {
        return inventoryWeightLimit;
    }

    public bool AddItem(WorldItem item)
    {
        int numItems = item.GetNumberOfItems();
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

        ItemInstance itemPickedUp = item.CreateItemInstance();
        itemPickedUp.SetProperty(ItemAttributeKey.NumItemsInStack, numItems);
        bool successCheck = AddItem(itemPickedUp);

        if (partialOnly)
        {
            item.ChangeNumberOfItems(-numItems);
            return false;
        }

        return successCheck && !partialOnly;
    }

    public bool AddItem(ItemInstance itemInstance)
    {
        int numItems = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
        float itemTotalWeight = itemInstance.sharedData.Weight * numItems;

        // Check if the total weight of the items can be added to the inventory
        if (itemTotalWeight > GetInventoryWeightLimit() - currentWeight)
        {
            Debug.Log("Not enough weight capacity to add these items");
            return false; // Not enough weight capacity to add these items
        }

        bool updated = false; // Flag to track if the inventory was updated

        // If the item is stackable, try to add it to existing slots with the same item
        if (itemInstance.sharedData.stackable)
        {
            int remainingItems = FillExistingStacks(itemInstance);
            if (remainingItems < numItems)
            {
                updated = true;  // Update occurred if we added some items to existing stacks
            }
            numItems = remainingItems;
        }

        // If there are items left after trying to stack them in existing slots, or if the item is not stackable
        if (numItems > 0)
        {
            updated = FillEmptySlots(itemInstance) || updated;
        }

        // Update listeners if any changes have occurred
        if (updated)
        {
            OnInventoryChanged?.Invoke();
        }

        return updated;
    }

    private int FillExistingStacks(ItemInstance itemInstance)
    {
        int numItems = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
        foreach (InventorySlot slot in inventorySlots)
        {
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.itemInstance.sharedData == itemInstance.sharedData)
            {
                int spaceLeftInSlot = itemInSlot.itemInstance.sharedData.maxStackSize - itemInSlot.GetItemCount();
                if (spaceLeftInSlot > 0)
                {
                    int itemsToAdd = Mathf.Min(spaceLeftInSlot, numItems);
                    if (itemInstance.sharedData.stackable)
                    {
                        itemInSlot.AddToItemCount(itemsToAdd);
                    }
                    /*else
                    {
                        itemInSlot.ChangeItemCount(itemsToAdd);
                    }*/
                    UpdateWeight(itemInstance.sharedData.Weight * itemsToAdd);
                    numItems -= itemsToAdd;
                    itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, numItems);

                    if (numItems == 0) break; // All items added, exit loop
                }
            }
        }
        return numItems; // Return the number of items left to add
    }

    private bool FillEmptySlots(ItemInstance itemInstance)
    {
        int numItems = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
        foreach (InventorySlot slot in inventorySlots)
        {
            if (!slot.HasItem())
            {
                int itemsToAdd = itemInstance.sharedData.stackable ? Mathf.Min(itemInstance.sharedData.maxStackSize, numItems) : 1;
                CreateNewItem(itemInstance, slot);
                //UpdateWeight(itemInstance.sharedData.Weight * itemsToAdd);
                numItems -= itemsToAdd;

                if (numItems == 0) return true; // Successfully added all items
            }
        }

        if (numItems > 0)
        {
            Debug.Log("Inventory is full or not enough slots to accommodate all items.");
            return false; // Not all items could be added
        }

        return true; // All items were added
    }

    public int GetNumberOfItemsOfType(ItemType type)
    {
        int numItems = 0;
        foreach(InventorySlot inventorySlot in inventorySlots)
        {
            InventoryItem itemInSlot = inventorySlot.GetItemInSlot();
            if (itemInSlot != null && inventorySlot.GetItemInSlot().GetItemType() == type)
            {
                if (inventorySlot.GetItemInSlot().itemInstance.sharedData.stackable)
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
    public bool RemoveItem(ItemInstance itemInstance)
    {
        Debug.Log("This Function has NOT Been Implemented");
        return true;
    }

    public bool RemoveItemOfType(ItemType type, int numItems = 1)
    {
        foreach (InventorySlot inventorySlot in inventorySlots)
        {
            InventoryItem itemInSlot = inventorySlot.GetItemInSlot();
            if (itemInSlot != null && inventorySlot.GetItemInSlot().GetItemType() == type)
            {
                if (itemInSlot.GetItemCount() > numItems)
                {
                    itemInSlot.AddToItemCount(-numItems);
                    UpdateWeight(-(numItems * itemInSlot.itemInstance.sharedData.Weight));
                    return true;
                } else if (itemInSlot.GetItemCount() == numItems)
                {
                    inventorySlot.RemoveItemFromSlot();
                    Destroy(itemInSlot.gameObject);
                    return true;
                } else
                {
                    inventorySlot.RemoveItemFromSlot();
                    Destroy(itemInSlot.gameObject);
                    numItems -= itemInSlot.GetItemCount();
                }
            }
        }
        return false;
    }

    protected void CreateNewItem(ItemInstance itemInstance, InventorySlot inventorySlot)
    {
        GameObject newItem = Instantiate(inventoryItemPrefab, inventorySlot.itemSlot);
        InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
        inventoryItem.name = itemInstance.sharedData.DisplayName;
        inventoryItem.InitializeItem(itemInstance);
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

    public void DropItem(ItemInstance itemInstance)
    {
        if (OnItemDropped != null)
            OnItemDropped(itemInstance);
    }
}
