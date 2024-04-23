using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public List<SerializableItemData> inventoryItems;
    public List<SerializableItemData> equippedItems;
    // Add additional fields as necessary, such as player stats, position, etc.
    // You could include methods here for easy loading and saving of data

    public PlayerData(PlayerInventory playerInventory)
    {
        inventoryItems = new List<SerializableItemData>();
        equippedItems = new List<SerializableItemData>();
        foreach (InventorySlot inventorySlot in playerInventory.inventorySlots)
        {
            if (inventorySlot.HasItem())
            {
                ItemInstance itemInstance = inventorySlot.GetItemInSlot().itemInstance;
                SharedItemData itemData = itemInstance.sharedData;
                int quantity = inventorySlot.GetItemInSlot().GetItemCount();
                inventoryItems.Add(SerializableItemData.FromSharedItemData(itemData, quantity));
            }
        }
        foreach(GearSlot gearSlot in playerInventory.GetGearSlots())
        {
            if (gearSlot.HasItem())
            {
                SharedItemData itemData = gearSlot.GetItemInSlot().itemInstance.sharedData;
                equippedItems.Add(SerializableItemData.FromSharedItemData(itemData, 1));
            }
            else
            {
                equippedItems.Add(CreateEmptyGearSlotData());
            }
        }
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static PlayerData FromJson(string json)
    {
        return JsonUtility.FromJson<PlayerData>(json);
    }

    private SerializableItemData CreateEmptyGearSlotData()
    {
        // Returns a default SerializableItemData that signifies an empty slot
        return new SerializableItemData
        {
            ID = "empty",  // Use a special ID or another property to indicate an empty slot
            DisplayName = "Empty Slot",
            ItemDescription = "No item equipped in this slot.",
            ItemType = ItemType.OTHER,  // Assuming you have a NONE type or similar
            Rarity = Rarity.COMMON,  // Default or irrelevant value
            Weight = 0,
            Stackable = false,
            MaxStackSize = 0,
            Quantity = 0  // Quantity zero to signify no item
        };
    }
}