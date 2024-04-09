using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventory : Inventory
{
    public static PlayerInventory instance;
    public GameObject backpackInventory;
    [SerializeField] List<GearSlot> gearSlots;
    [SerializeField] List<GearSlot> weaponSlots;
    [SerializeField] TMP_Text weightText; // "BACKPACK 0.0/0.0"

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    new protected void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool currentStatus = backpackInventory.activeSelf;
            backpackInventory.SetActive(!currentStatus);
            Character.SetUserInputStatus(currentStatus);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            foreach (BaseItem startItem in startItems)
            {
                AddItem(startItem);
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlaceItem(inventorySlots[0].GetItemInSlot(), inventorySlots[7]);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            PlaceItem(inventorySlots[1].GetItemInSlot(), inventorySlots[2]);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            RemoveAllItemsFromEachSlot();
        }
    }

    public override void UpdateWeight(float amount)
    {
        base.UpdateWeight(amount);
        weightText.text = "BACKPACK " + currentWeight.ToString() + "/" + inventoryWeightLimit;
    }

    public void StartShowSlotAcceptability(InventoryItem inventoryItem)
    {
        foreach (GearSlot weaponSlot in gearSlots)
        {
            weaponSlot.DisplayItemIndication(inventoryItem.GetItemType());
        }
    }

    public void EndShowSlotAcceptability(InventoryItem inventoryItem)
    {
        foreach (GearSlot weaponSlot in gearSlots)
        {
            weaponSlot.ResetItemIndication();
        }
    }

    public override void StartInventoryItemMoved(InventoryItem inventoryItem)
    {
        StartShowSlotAcceptability(inventoryItem);
    }

    public override void EndInventoryItemMoved(InventoryItem inventoryItem)
    {
        EndShowSlotAcceptability(inventoryItem);
    }

    private void RemoveAllItemsFromEachSlot()
    {
        foreach(InventorySlot inventorySlot in inventorySlots)
        {
            if (inventorySlot.GetItemInSlot() != null)
            {
                InventoryItem item = inventorySlot.RemoveItemFromSlot();
                Destroy(item.gameObject);
            }
        }
    }

    public override void QuickEquip(InventorySlot inventorySlot)
    {
        InventoryItem itemToEquip = inventorySlot.GetItemInSlot();

        // First, anything that is other, can't be equipped.
        // Later, there may be other "Types" that can't be equipped, but for now this works.
        if (itemToEquip.GetItemType() == ItemType.OTHER)
        {
            return;
        }

        // If the slot clicked is an inventory slot, equip item to gearSlot
        if (inventorySlots.Contains(inventorySlot)) {
            // Switch it with a gear slot\
            GearSlot gearSlotMatch = null;
            // If it's a weapon, prefer an empty slot, else, the first slot
            if (itemToEquip.GetItemType() == ItemType.PRIMARY_WEAPON)
            {
                // Pick first slot if it's empty
                if (!weaponSlots[0].HasItem())
                {
                    gearSlotMatch = weaponSlots[0];
                }
                // Pick second slot if it's empty
                else if (!weaponSlots[1].HasItem())
                {
                    gearSlotMatch = weaponSlots[1];
                } else // Else, default to first slot
                {
                    gearSlotMatch = weaponSlots[0];
                }
            } else
            {
                foreach (GearSlot gearSlot in gearSlots)
                {
                    if (gearSlot.GetItemType() == itemToEquip.GetItemType())
                    {
                        gearSlotMatch = gearSlot;
                        break;
                    }
                }
            }

            gearSlotMatch.Swap(itemToEquip);
        } else // If the slot was a gear slot then swap into inventory
        {
            // Add it to the inventory, find empty slot
            InventorySlot inventorySlotMatch = null;
            foreach (InventorySlot tempInventorySlot in inventorySlots)
            {
                if (!tempInventorySlot.HasItem())
                {
                    inventorySlotMatch = tempInventorySlot;
                    break;
                } 
            }

            if (inventorySlot = null)
            {
                Debug.Log("ERROR: No empty slots");
                // TODO: Add more slots dynamically
                // Make inventory a scrollable window
            }
            inventorySlotMatch.Swap(itemToEquip);
        }
    }
}
