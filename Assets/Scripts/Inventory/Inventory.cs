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
    [SerializeField] protected float currentWeight;
    [SerializeField] public static List<Color> rarityColors;

    public virtual float GetInventoryWeightLimit()
    {
        return inventoryWeightLimit;
    }

    public float GetCurrentInventoryWeight()
    {
        return currentWeight;
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
            int itemsLeftToAdd = FillEmptySlots(itemInstance);
            if (itemsLeftToAdd < numItems)
            {
                updated = true;  // Update occurred if we added some items to new slots
            }
        }

        return updated;
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

    public bool AddItem(InventorySlot slotToAddTo, InventoryItem itemToAdd)
    {
        // If itemslot has item, swap, unless stackable and same item type
        if (slotToAddTo.HasItem())
        {
            InventoryItem itemInSlot = slotToAddTo.GetItemInSlot();
            InventorySlot otherSlot = itemToAdd.GetCurrentInventorySlot();
            if (itemToAdd.itemInstance.sharedData.Stackable &&
                itemToAdd.GetItemType() == itemInSlot.GetItemType())
            {
                // TODO: Need to do something like fill it with what it can
                //return false;
            }

            InventoryItem itemAlreadyHere = itemInSlot;
            float weightAfterSwap = currentWeight;
            float weightLimitAfterSwap = GetInventoryWeightLimit();

            // Check to see if it's too heavy for inventory
            if (slotToAddTo.ContributesToWeight())
            {
                // If the other slot is the backpack slot then recalculate the inventory size
                if ((slotToAddTo as GearSlot || otherSlot as GearSlot) && itemInSlot.GetItemType() == ItemType.BACKPACK)
                {
                    weightLimitAfterSwap +=
                        ((BackpackItem)itemAlreadyHere.itemInstance.sharedData).CarryCapacity -
                        ((BackpackItem)itemToAdd.itemInstance.sharedData).CarryCapacity;
                }

                weightAfterSwap = weightAfterSwap + itemToAdd.GetTotalWeight() - itemAlreadyHere.GetTotalWeight();
            }
            if (otherSlot.ContributesToWeight())
            {
                // If the this slot is the backpack slot then recalculate the inventory size
                if ((slotToAddTo as GearSlot || otherSlot as GearSlot) && itemInSlot.GetItemType() == ItemType.BACKPACK)
                {
                    weightLimitAfterSwap +=
                        ((BackpackItem)itemToAdd.itemInstance.sharedData).CarryCapacity -
                        ((BackpackItem)itemAlreadyHere.itemInstance.sharedData).CarryCapacity;
                }

                weightAfterSwap = weightAfterSwap + itemAlreadyHere.GetTotalWeight() - itemToAdd.GetTotalWeight();
            }

            if (weightAfterSwap > weightLimitAfterSwap)
            {
                return false;
            }

            Swap(slotToAddTo, itemToAdd);
        }
        else
        {
            InventorySlot otherSlot = itemToAdd.GetCurrentInventorySlot();
            Inventory otherInventory = otherSlot.GetInventory();

            // Check to see if it's too heavy for inventory
            if (slotToAddTo.ContributesToWeight())
            {
                if (!otherSlot.ContributesToWeight())
                {
                    float weightLimitAfterSwap = GetInventoryWeightLimit();
                    // If the this slot is the backpack slot then recalculate the inventory size
                    if (itemToAdd.itemInstance.sharedData.ItemType == ItemType.BACKPACK &&
                        otherSlot as GearSlot)
                    {
                        weightLimitAfterSwap -=
                            ((BackpackItem)itemToAdd.itemInstance.sharedData).CarryCapacity;
                    }

                    if (currentWeight + itemToAdd.GetTotalWeight() > weightLimitAfterSwap)
                    {
                        // Try Splitting it if its stackable
                        if (itemToAdd.itemInstance.sharedData.Stackable)
                        {
                            MoveAsManyAsYouCan(slotToAddTo, itemToAdd);
                        }
                        return false;
                    }
                }
            }

            if (slotToAddTo.ContributesToWeight())
            {
                UpdateWeight(itemToAdd.GetTotalWeight());
            }
            if (otherSlot.ContributesToWeight())
            {
                otherInventory.UpdateWeight(-itemToAdd.GetTotalWeight());
            }
        }

        return true;
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
        if (inventorySlot.ContributesToWeight())
        {
            UpdateWeight(inventoryItem.GetTotalWeight());
        }
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

            if (otherSlot.ContributesToWeight())
            {
                otherSlot.GetInventory().UpdateWeight(inventoryItemAlreadyHere.GetTotalWeight());
                otherSlot.GetInventory().UpdateWeight(-incomingItem.GetTotalWeight());
            }
            if (inventorySlot.ContributesToWeight())
            {
                UpdateWeight(incomingItem.GetTotalWeight());
                UpdateWeight(-inventoryItemAlreadyHere.GetTotalWeight());
            }

            inventoryItemAlreadyHere.DoThingsAfterMove();
            incomingItem.DoThingsAfterMove();
        }
        else
        {
            InventorySlot otherSlot = incomingItem.GetCurrentInventorySlot();
            otherSlot.RemoveItemFromSlot();
            if (otherSlot.ContributesToWeight())
            {
                otherSlot.GetInventory().UpdateWeight(-incomingItem.GetTotalWeight());
            }
            inventorySlot.SetItemInSlotAfterDrag(incomingItem);
            if (inventorySlot.ContributesToWeight())
            {
                inventorySlot.GetInventory().UpdateWeight(incomingItem.GetTotalWeight());
            }
            incomingItem.DoThingsAfterMove();
        }
    }

    public virtual void QuickEquip(InventorySlot inventorySlot)
    {
        AddItemToEarliestEmptySlot(inventorySlot.GetItemInSlot());
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

    public void MoveAsManyAsYouCan(InventorySlot inventorySlot, InventoryItem inventoryItem)
    {
        if (!inventoryItem.itemInstance.sharedData.Stackable) return;

        InventorySlot currentSlot = inventoryItem.GetCurrentInventorySlot();

        int numItemsInStack = (int)inventoryItem.itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);

        // Calculate the available weight capacity
        float availableWeight = GetInventoryWeightLimit() - currentWeight;

        // Calculate the maximum number of items that can be added based on weight
        int maxItemsByWeight = (int)(availableWeight / inventoryItem.itemInstance.sharedData.Weight);

        if (maxItemsByWeight <= 0) return;

        // Remove the correct number of items from the existing property, update the weight in the inventory accordingly, then update the stats.
        inventoryItem.itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, numItemsInStack - maxItemsByWeight);
        if (currentSlot.partOfPlayerInventory && currentSlot.ContributesToWeight())
        {
            UpdateWeight(inventoryItem.itemInstance.sharedData.Weight * -maxItemsByWeight);
        }
        inventoryItem.GetCurrentInventorySlot().RefreshItemStats();

        // Create new itemInstance, set it's number, fill empty slot with it.
        ItemInstance newItem = new ItemInstance(inventoryItem.itemInstance.sharedData);
        if ((!inventorySlot.partOfPlayerInventory && inventorySlot.ContributesToWeight()))
        {
            // Probably need to do something about weight in this instance, we'll see
        }
        newItem.SetProperty(ItemAttributeKey.NumItemsInStack, maxItemsByWeight);
        FillEmptySlots(newItem);
    }
}
