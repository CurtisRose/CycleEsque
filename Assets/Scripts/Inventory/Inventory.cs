using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public BaseItem[] startItems;
    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;
    public float inventoryWeightLimit;
    public float currentWeight;

    protected void Start()
    {
        foreach (BaseItem startItem in startItems)
        {
            AddItem(startItem);
        }
    }

    public bool AddItem(BaseItem item)
    {
        if (item.Weight > inventoryWeightLimit - currentWeight)
        {
            return false;
        }

        // Add item weight to current weight
        //UpdateWeight(item.Weight);

        // Check if any slot already has item with count lower than the stack size
        if (item.stackable)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                InventorySlot slot = inventorySlots[i];
                InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
                if (itemInSlot != null &&
                    itemInSlot.item == item &&
                    itemInSlot.count < itemInSlot.item.maxStackSize)
                {
                    itemInSlot.count++;
                    itemInSlot.RefreshItemCount();
                    return true;
                }
            }
        }

        // Find an empty slot
        for (int i = 0; i < inventorySlots.Length; i++)
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

    public virtual void UpdateWeight(float amount)
    {
        currentWeight += amount;
    }

    public bool RemoveItem(BaseItem item)
    {
        return true;
    }

    protected void CreateNewItem(BaseItem item, InventorySlot inventorySlot)
    {
        GameObject newItem = Instantiate(inventoryItemPrefab, inventorySlot.itemSlot);
        InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
        inventoryItem.name = item.DisplayName;
        inventoryItem.InitializeItem(item);
        inventorySlot.SetItemInSlotAfterDrag(inventoryItem);
    }

    public virtual void StartInventoryItemMoved(InventoryItem inventoryItem)
    {

    }

    public virtual void EndInventoryItemMoved(InventoryItem inventoryItem)
    {

    }

    public void PlaceItem(InventoryItem item, InventorySlot inventorySlot)
    {
        if (item != null)
        {
            inventorySlot.Swap(item);
        }
    }
}
