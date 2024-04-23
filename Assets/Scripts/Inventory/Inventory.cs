using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct InventoryStartItem
{
    public SharedItemData itemData;
    public int quantity;
}

public class Inventory : MonoBehaviour
{
    public List<InventorySlot> inventorySlots;
    public GameObject inventoryItemPrefab;
    [SerializeField] private float inventoryWeightLimit;
    public float currentWeight;
    [SerializeField] public static List<Color> rarityColors;

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
            if (item.GetBaseItem().Stackable)
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

    public int HowManyItemsCanBeAdded(ItemInstance itemInstance)
    {
        int numItems = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);

        // Calculate the available weight capacity
        float availableWeight = GetInventoryWeightLimit() - currentWeight;

        // Calculate the maximum number of items that can be added based on weight
        int maxItemsByWeight = (int)(availableWeight / itemInstance.sharedData.Weight);

        // Determine the actual number of items that can be added
        int itemsToAdd = Mathf.Min(numItems, maxItemsByWeight);

        return itemsToAdd;
    }

    public virtual bool AddItem(ItemInstance itemInstance)
    {
            int numItems = HowManyItemsCanBeAdded(itemInstance);

        if (numItems <= 0)
        {
            Debug.Log("No Room In Inventory");
            return false;
        }

        bool updated = false; // Flag to track if the inventory was updated

        // If the item is stackable, try to add it to existing slots with the same item
        if (itemInstance.sharedData.Stackable)
        {
            int startItems = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
            int remainingItems = FillExistingStacks(itemInstance);
            int itemsAdded = startItems - remainingItems;
            UpdateWeight(itemInstance.sharedData.Weight * itemsAdded);
            if (remainingItems < numItems)
            {
                updated = true;  // Update occurred if we added some items to existing stacks
            }
            numItems = remainingItems;
        }

        // If there are items left after trying to stack them in existing slots, or if the item is not stackable
        if (numItems > 0)
        {
            int itemsLeftToAdd =  FillEmptySlots(itemInstance);
            if (itemsLeftToAdd < numItems)
            {
                updated = true;  // Update occurred if we added some items to new slots
            }
        }

        return updated;
    }

    protected int FillExistingStacks(ItemInstance itemInstance)
    {
        int numItems = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
        foreach (InventorySlot slot in inventorySlots)
        {
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.itemInstance.sharedData == itemInstance.sharedData)
            {
                int spaceLeftInSlot = itemInSlot.itemInstance.sharedData.MaxStackSize - itemInSlot.GetItemCount();
                if (spaceLeftInSlot > 0)
                {
                    int itemsToAdd = Mathf.Min(spaceLeftInSlot, numItems);
                    if (itemInstance.sharedData.Stackable)
                    {
                        itemInSlot.AddToItemCount(itemsToAdd);
                        numItems -= itemsToAdd;
                        itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, numItems);
                    }
                    if (numItems == 0) break; // All items added, exit loop
                }
            }
        }
        return numItems; // Return the number of items left to add
    }

    public int FillEmptySlots(ItemInstance itemInstance)
    {
        int numItems = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);

        // Calculate the available weight capacity
        float availableWeight = GetInventoryWeightLimit() - currentWeight;

        // Calculate the maximum number of items that can be added based on weight
        int maxItemsByWeight = (int)(availableWeight / itemInstance.sharedData.Weight);

        // Determine the actual number of items that can be added
        int itemsToAdd = Mathf.Min(numItems, maxItemsByWeight);

        if (itemsToAdd <= 0)
        {
            Debug.Log("Not enough weight capacity to add these items");
            return numItems; // Not enough weight capacity to add any of the items, return the original number
        }

        foreach (InventorySlot slot in inventorySlots)
        {
            if (!slot.HasItem() && itemsToAdd > 0)
            {
                int itemsThatCanBeAdded = itemInstance.sharedData.Stackable ? Mathf.Min(itemInstance.sharedData.MaxStackSize, itemsToAdd) : 1;
                ItemInstance newItemInstance = itemInstance.Clone(); // Create a deep copy of itemInstance
                newItemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, itemsThatCanBeAdded);

                // Create item adds the weight at some point.
                CreateNewItem(newItemInstance, slot);  // Create a new item in the slot
                //UpdateWeight(newItemInstance.sharedData.Weight * itemsThatCanBeAdded);  // Update the total weight

                itemsToAdd -= itemsThatCanBeAdded; // Reduce the number of items left to add
                if (itemsToAdd == 0) return 0; // Successfully added all items
            }
        }

        if (itemsToAdd > 0)
        {
            Debug.Log("Inventory is full or not enough slots to accommodate all items.");
        }

        return itemsToAdd; // Return the number of items left that could not be added
    }

    public int GetNumberOfItemsOfType(ItemType type)
    {
        int numItems = 0;
        foreach(InventorySlot inventorySlot in inventorySlots)
        {
            InventoryItem itemInSlot = inventorySlot.GetItemInSlot();
            if (itemInSlot != null && inventorySlot.GetItemInSlot().GetItemType() == type)
            {
                if (inventorySlot.GetItemInSlot().itemInstance.sharedData.Stackable)
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
        InventorySlot emptySlot = FindEarliestEmptySlot(inventoryItem);
        if (emptySlot != null)
        {
            Swap(emptySlot,inventoryItem);
        }
        else
        {
            //Debug.Log("No earlier empty slot available.");
        }
    }
    public InventorySlot FindEarliestEmptySlot(InventoryItem inventoryItem)
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot == inventoryItem.GetCurrentInventorySlot())
            {
                break; // Stop searching when reaching the current slot of the item
            }
            if (!slot.HasItem())
            {
                return slot; // Return the first empty slot found before the current slot
            }
        }
        return null; // Return null if no suitable slot is found
    }

    public InventorySlot FindEarliestEmptySlot()
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (!slot.HasItem())
            {
                return slot; // Return the first empty slot found before the current slot
            }
        }
        return null; // Return null if no suitable slot is found
    }

    public void PlaceItem(InventoryItem item, InventorySlot inventorySlot)
    {
        if (item != null)
        {

            Swap(inventorySlot, item);
        }
    }

    public virtual void Swap(InventorySlot inventorySlot, InventoryItem incomingItem)
    {
        if (inventorySlot.HasItem())
        {
            InventoryItem inventoryItemAlreadyHere = inventorySlot.GetItemInSlot();
            InventorySlot otherSlot = incomingItem.GetCurrentInventorySlot();
            otherSlot.RemoveItemFromSlot();
            inventorySlot.RemoveItemFromSlot();

            otherSlot.SetItemInSlotAfterDrag(inventoryItemAlreadyHere);
            inventorySlot.SetItemInSlotAfterDrag(incomingItem);

            inventoryItemAlreadyHere.DoThingsAfterMove();
            incomingItem.DoThingsAfterMove();
        }
        else
        {
            InventorySlot otherSlot = incomingItem.GetCurrentInventorySlot();
            otherSlot.RemoveItemFromSlot();
            inventorySlot.SetItemInSlotAfterDrag(incomingItem);
            incomingItem.DoThingsAfterMove();
        }
    }

    public virtual void QuickEquip(InventorySlot inventorySlot)
    {
        // Possibly, this can be used to right click items into earlier slots.
        // For now only used in the player inventory class
    }

    public virtual void DropItem(ItemInstance itemInstance)
    {

    }

    public void SplitInventoryItem(InventoryItem inventoryItem)
    {
        if (inventoryItem.itemInstance.sharedData.Stackable)
        {
            if ((int)inventoryItem.itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack) <= 1)
            {
                return;
            }
            InventorySlot otherSlot = FindEarliestEmptySlot();
            // If there isn't another empty slot, then out of luck.
            if (otherSlot == null)
            {
                return;
            }
            // There should be no weight considerations since it's already in the bag
            int numItems = (int)inventoryItem.itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
            int newStackNum = Mathf.FloorToInt(numItems / 2);

            // Remove the correct number of items from the existing property, update the weight in the inventory accordingly, then update the stats.
            inventoryItem.itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, numItems - newStackNum);
            UpdateWeight(inventoryItem.itemInstance.sharedData.Weight * -newStackNum);
            inventoryItem.GetCurrentInventorySlot().RefreshItemStats();

            // Create new itemInstance, set it's number, fill empty slot with it.
            ItemInstance newItem = new ItemInstance(inventoryItem.itemInstance.sharedData);
            newItem.SetProperty(ItemAttributeKey.NumItemsInStack, newStackNum);
            FillEmptySlots(newItem);
        }
    }
}
