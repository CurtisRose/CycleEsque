using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : Inventory
{
    public static InventoryManager instance;
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
            foreach (Item startItem in startItems)
            {
                Debug.Log(AddItem(startItem));
            }
        }
    }

    public override void UpdateWeight(float amount)
    {
        base.UpdateWeight(amount);
        weightText.text = "BACKPACK " + currentWeight.ToString() + "/" + inventoryWeightLimit;
    }

    public override void OnItemStartDragged(InventoryItem inventoryItem)
    {
        foreach (GearSlot weaponSlot in weaponSlots)
        {
            weaponSlot.DisplayItemIndication(inventoryItem.GetItemType());
        }
    }

    public override void OnItemStopDragged(InventoryItem inventoryItem)
    {
        foreach (GearSlot weaponSlot in weaponSlots)
        {
            weaponSlot.ResetItemIndication();
        }
    }
}
