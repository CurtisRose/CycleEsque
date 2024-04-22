using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum GearSlotIdentifier { BACKPACK, ARMOR, HELMET, WEAPONSLOT1, WEAPONSLOT2 };

public class PlayerInventory : Inventory
{
    [SerializeField] protected InventoryStartItem[] startItems;
    public static PlayerInventory Instance;
    public GameObject backpackInventory;
    [SerializeField] List<GearSlot> gearSlots;
    [SerializeField] TMP_Text weightText; // "BACKPACK 0.0/0.0"

    public delegate void InventoryChanged();
    public event InventoryChanged OnInventoryChanged;

    public delegate void ItemDropped(ItemInstance itemInstance);
    public event ItemDropped OnItemDropped;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    new protected void Start()
    {
        if (PlayerInventoryMenu.Instance != null)
        {
            PlayerInventoryMenu.Instance.Open();
        }

        foreach (InventoryStartItem startItem in startItems)
        {
            //ItemInstance itemInstance = new ItemInstance(startItem);
            WorldItem testItem = PlayerItemSpawner.Instance.GetPrefab(startItem.itemData);
            ItemInstance testInstance = testItem.CreateNewItemInstance(startItem.itemData);
            if (startItem.itemData.Stackable)
            {
                testInstance.SetProperty(ItemAttributeKey.NumItemsInStack, startItem.quantity);
            }

            //itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, 1);
            AddItem(testInstance);
        }

        if (PlayerInventoryMenu.Instance != null)
        {
            PlayerInventoryMenu.Instance.Close();
        }
    }

    public override bool AddItem(ItemInstance itemInstance)
    {
        bool updated = base.AddItem(itemInstance);
        // Update listeners if any changes have occurred
        if (updated)
        {
            OnInventoryChanged?.Invoke();
        }
        return updated;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {

            if (PlayerInventoryMenu.Instance != null)
            {
                if (!PlayerInventoryMenu.Instance.IsOpen())
                {
                    MenuManager.Instance.OpenMenu(PlayerInventoryMenu.Instance);
                }
                else
                {
                    MenuManager.Instance.CloseMenu(PlayerInventoryMenu.Instance);
                }
            }
        }
        if (Input.GetKey(KeyCode.E))
        {
            ItemInstance ammo = new ItemInstance(startItems[5].itemData);
            ammo.SetProperty(ItemAttributeKey.NumItemsInStack, 1);
            AddItem(ammo);
        }
    }

    public override float GetInventoryWeightLimit()
    {
        if (gearSlots[(int)GearSlotIdentifier.BACKPACK].GetItemInSlot() != null)
        {
            BackpackItem backpack = (BackpackItem)gearSlots[(int)GearSlotIdentifier.BACKPACK].GetItemInSlot().itemInstance.sharedData;

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
        // Find the earliest slot to quick sort it in your inventory
        if (itemToEquip.GetItemType() >= ItemType.AMMO)
        {
            AddItemToEarliestEmptySlot(inventorySlot.GetItemInSlot());
            return;
        }

        // If the slot clicked is an inventory slot, equip item to gearSlot
        if (inventorySlots.Contains(inventorySlot)) {
            // Switch it with a gear slot\
            GearSlot gearSlotMatch = null;

            // If it's a weapon, prefer an empty slot, else, the first slot
            if (itemToEquip.GetItemType() == ItemType.WEAPON)
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

            if (gearSlotMatch == null)
            {
                Debug.LogError("Error: No gear slot matches this type. Gear slots are probably misconfigured.");
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
                    newCarryCapacity = base.GetInventoryWeightLimit() + ((BackpackItem)itemToEquip.itemInstance.sharedData).CarryCapacity;
                }

                if (newCarryCapacity >= weightAfterSwitch)
                {
                    weightCheck = true;
                }
            }

            if (weightCheck)
            {
                // First move the item to an earlier slot.
                AddItemToEarliestEmptySlot(itemToEquip);
                // Then swap them, that way the gear ends up in the earlier slot
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

    public override void DropItem(ItemInstance itemInstance)
    {
        if (OnItemDropped != null)
            OnItemDropped(itemInstance);
    }

    void OnValidate()
    {
        for (int i = 0; i < startItems.Length; i++)
        {
            if (startItems[i].quantity < 1)
                startItems[i].quantity = 1;
        }
    }
}
