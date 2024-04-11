using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum GearSlotIdentifier { BACKPACK, ARMOR, HELMET, WEAPONSLOT1, WEAPONSLOT2 };

public class PlayerInventory : Inventory
{
    public static PlayerInventory instance;
    public GameObject backpackInventory;
    [SerializeField] List<GearSlot> gearSlots;
    [SerializeField] TMP_Text weightText; // "BACKPACK 0.0/0.0"
    [SerializeField] Menu inventoryMenu;

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
        inventoryMenu.Open();
        base.Start();
        inventoryMenu.Close();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {

            if (!inventoryMenu.IsOpen())
            {
                MenuManager.Instance.OpenMenu(inventoryMenu);
            } else
            {
                MenuManager.Instance.CloseMenu(inventoryMenu);
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveAllItemsFromEachSlot();
        }
        if (Input.GetKey(KeyCode.E))
        {
            AddItem(startItems[5]);
        }
    }

    public override float GetInventoryWeightLimit()
    {
        if (gearSlots[(int)GearSlotIdentifier.BACKPACK].GetItemInSlot() != null)
        {
            BackpackItem backpack = (BackpackItem)gearSlots[(int)GearSlotIdentifier.BACKPACK].GetItemInSlot().item;

            return base.GetInventoryWeightLimit() + backpack.CarryCapacity;
        }
        return base.GetInventoryWeightLimit();
    }

    public override void UpdateWeight(float amount)
    {
        base.UpdateWeight(amount);
        UpdateWeightText();
    }

    public void UpdateWeightText()
    {
        weightText.text = "BACKPACK " + currentWeight.ToString() + "/" + GetInventoryWeightLimit();
    }

    public void StartShowSlotAcceptability(InventoryItem inventoryItem)
    {
        foreach (GearSlot gearSlot in gearSlots)
        {
            gearSlot.DisplayItemIndication(inventoryItem.GetItemType());
        }
    }

    public void EndShowSlotAcceptability(InventoryItem inventoryItem)
    {
        foreach (GearSlot gearSlot in gearSlots)
        {
            gearSlot.ResetItemIndication();
        }
    }

    public override void StartInventoryItemMoved(InventoryItem inventoryItem)
    {
        StartShowSlotAcceptability(inventoryItem);
    }

    public override void EndInventoryItemMoved(InventoryItem inventoryItem)
    {
        EndShowSlotAcceptability(inventoryItem);
        UpdateWeightText();
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
                if (!gearSlots[(int)GearSlotIdentifier.WEAPONSLOT1].HasItem())
                {
                    gearSlotMatch = gearSlots[(int)GearSlotIdentifier.WEAPONSLOT1];
                }
                // Pick second slot if it's empty
                else if (!gearSlots[(int)GearSlotIdentifier.WEAPONSLOT2].HasItem())
                {
                    gearSlotMatch = gearSlots[(int)GearSlotIdentifier.WEAPONSLOT2];
                } else // Else, default to first slot
                {
                    gearSlotMatch = gearSlots[(int)GearSlotIdentifier.WEAPONSLOT1];
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

            // Do Weight Check Before Swapping, this is swapping inventory item into gear
            bool weightCheck = false;
            {
                // Get the weight difference
                float weightAfterSwitch = currentWeight - itemToEquip.GetTotalWeight();
                if (gearSlotMatch.GetItemInSlot() != null)
                {
                    weightAfterSwitch += gearSlotMatch.GetItemInSlot().GetTotalWeight();
                }
                float newCarryCapacity = GetInventoryWeightLimit();
                // Then check if it was a backpack switch to check the new carry weight
                if (itemToEquip.GetItemType() == ItemType.BACKPACK)
                {
                    newCarryCapacity = base.GetInventoryWeightLimit() + ((BackpackItem)itemToEquip.item).CarryCapacity;
                }

                if (newCarryCapacity >= weightAfterSwitch)
                {
                    weightCheck = true;
                }
            }

            if (weightCheck)
            {
                gearSlotMatch.Swap(itemToEquip);
            }
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

            // Do Weight Check Before Swapping, this is swapping gear into the inventory
            bool weightCheck = false;
            {
                // Get the weight difference
                float weightAfterSwitch = currentWeight + itemToEquip.GetTotalWeight();

                float newCarryCapacity = GetInventoryWeightLimit();
                // Then check if it was a backpack switch to check the new carry weight
                if (itemToEquip.GetItemType() == ItemType.BACKPACK)
                {
                    newCarryCapacity = base.GetInventoryWeightLimit();
                }

                if (newCarryCapacity >= weightAfterSwitch)
                {
                    weightCheck = true;
                }
            }

            if (weightCheck)
            {
                inventorySlotMatch.Swap(itemToEquip);
            }
        }
        UpdateWeightText();
    }

    public List<GearSlot> GetGearSlots()
    {
        return gearSlots;
    }

    public GearSlot GetGearSlot(GearSlotIdentifier identifier)
    {
        return gearSlots[(int)identifier];
    }
}
