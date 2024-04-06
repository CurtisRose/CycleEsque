using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : Inventory
{
    public static InventoryManager instance;
    public GameObject backpackInventory;

    int selectedSlot = -1;
    int toolBarLength = 5;

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
        ChangeSelectedSlot(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool currentStatus = backpackInventory.activeSelf;
            backpackInventory.SetActive(!currentStatus);
            Character.SetUserInputStatus(currentStatus);
        }
        if (Input.inputString != null)
        {
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if (isNumber)
            {
                ChangeSelectedSlot(number - 1);
            }
        }
    }

    void ChangeSelectedSlot(int newSlot)
    {
        if (selectedSlot >= 0)
        {
            inventorySlots[selectedSlot].Deselect();
        }
        if (newSlot < toolBarLength) {
            selectedSlot = newSlot;
            inventorySlots[selectedSlot].Select();
        }
    }

    public Item GetSelectedItem(bool useDrop)
    {
        InventorySlot slot = inventorySlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            Item item = itemInSlot.item;
            if(useDrop)
            {
                itemInSlot.count--;
                itemInSlot.RefreshItemCount();
                if (itemInSlot.count <= 0)
                {
                    Destroy(itemInSlot.gameObject);
                }
            }
            return item;
        }
        return null;
    }
}
