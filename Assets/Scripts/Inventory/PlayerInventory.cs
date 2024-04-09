using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventory : Inventory
{
    public static PlayerInventory instance;
    public GameObject backpackInventory;
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
        foreach (GearSlot weaponSlot in weaponSlots)
        {
            weaponSlot.DisplayItemIndication(inventoryItem.GetItemType());
        }
    }

    public void EndShowSlotAcceptability(InventoryItem inventoryItem)
    {
        foreach (GearSlot weaponSlot in weaponSlots)
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
}
