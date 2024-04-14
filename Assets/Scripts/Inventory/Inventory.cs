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

    public delegate void InventoryChanged();
    public event InventoryChanged OnInventoryChanged;

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
        if (item.Weight * numItems > temp - currentWeight)
        {
            return false; // Not enough weight capacity to add these items
        }

        bool updated = false;
        // Check if any slot already has the item and can hold more
        if (item.stackable)
        {
            InventorySlot firstEmptySlot = null;
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                InventorySlot slot = inventorySlots[i];
                InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                // Check if the slot already contains the item and is not full
                if (itemInSlot != null && itemInSlot.item == item)
                {
                    int spaceLeftInSlot = itemInSlot.item.maxStackSize - itemInSlot.GetItemCount();
                    if (spaceLeftInSlot > 0)
                    {
                        int itemsToAdd = Mathf.Min(spaceLeftInSlot, numItems);
                        itemInSlot.ChangeItemCount(itemsToAdd);
                        UpdateWeight(item.Weight * itemsToAdd);
                        numItems -= itemsToAdd;
                        updated = true;

                        if (numItems <= 0)
                        {
                            if (OnInventoryChanged != null)
                                OnInventoryChanged();
                            return true;
                        }
                    }
                }

                // Remember the first empty slot if we need to add a new item there
                if (!slot.HasItem() && firstEmptySlot == null)
                {
                    firstEmptySlot = slot;
                }
            }

            // If there are still items left to add, use the first empty slot
            if (firstEmptySlot != null && numItems > 0)
            {
                CreateNewItem(item, firstEmptySlot, numItems);
                if (OnInventoryChanged != null)
                    OnInventoryChanged();
                return true;
            }
        }
        else
        {
            // Non-stackable item, simply find an empty slot
            foreach (InventorySlot slot in inventorySlots)
            {
                if (!slot.HasItem())
                {
                    CreateNewItem(item, slot, numItems);
                    if (OnInventoryChanged != null)
                        OnInventoryChanged();
                    return true;
                }
            }
        }

        // If no suitable slot is found and there are leftover items, log the full inventory
        if (numItems > 0)
        {
            Debug.Log("Inventory Is Full or not enough slots to accommodate all items.");
            return false;
        }

        // If inventory changes occurred, update listeners
        if (updated)
        {
            if (OnInventoryChanged != null)
                OnInventoryChanged();
            return true;
        }

        return false; // Default return, though logically unreachable in current setup
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

    public bool RemoveItemOfType(ItemType type, int numItems = 1)
    {
        foreach (InventorySlot inventorySlot in inventorySlots)
        {
            InventoryItem itemInSlot = inventorySlot.GetItemInSlot();
            if (itemInSlot != null && inventorySlot.GetItemInSlot().GetItemType() == type)
            {
                if (itemInSlot.GetItemCount() > numItems)
                {
                    itemInSlot.ChangeItemCount(-numItems);
                    UpdateWeight(-(numItems * itemInSlot.item.Weight));
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
