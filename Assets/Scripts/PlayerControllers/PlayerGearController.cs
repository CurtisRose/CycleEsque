using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGearController : MonoBehaviour
{
    [SerializeField] PlayerInventory playerInventory;
    [SerializeField] bool selectedFirstSlot;
    [SerializeField] Gun currentGunHeld = null;

    // GearSlotIdentifier { BACKPACK, ARMOR, HELMET, WEAPONSLOT1, WEAPONSLOT2 }
    [SerializeField] WorldItem[] gearItems;
    [SerializeField] List<Transform> gearStorageLocations;
    [SerializeField] Transform weaponPositionHands;
    [SerializeField] Transform weaponPositionHip;
    [SerializeField] Transform head;
    [SerializeField] float throwForce;
    [SerializeField] Transform throwPosition;


    private void Awake()
    {
        gearItems = new WorldItem[5];
        foreach(GearSlot gearSlot in playerInventory.GetGearSlots())
        {
            gearSlot.OnGearSlotsChanged += GearSlotChange;
        }
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Check if there's any scroll input
        if (scroll != 0f)
        {
            selectedFirstSlot = !selectedFirstSlot;
            SwitchGuns();
        }


        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedFirstSlot = true;
            SwitchGuns();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedFirstSlot = false;
            SwitchGuns();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (InventoryItem.CurrentHoveredItem != null)
            {
                //InventoryItem.CurrentHoveredItem
                //InventoryItem.CurrentHoveredItem.item
                InventoryItem.CurrentHoveredItem.GetCurrentInventorySlot().RemoveItemFromSlot();
                WorldItem itemBeingDropped = Instantiate<WorldItem>(InventoryItem.CurrentHoveredItem.item.itemPrefab, throwPosition.position, Quaternion.identity);
                // Maybe yeet it a little bit
                itemBeingDropped.GetComponent<Rigidbody>().AddForce(head.forward * throwForce * Time.deltaTime, ForceMode.Impulse);
                
                Destroy(InventoryItem.CurrentHoveredItem.gameObject);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            // Reload Gun
            int numberOfRoundsAvailable = 100;
            int numberOfRoundsUsed = currentGunHeld.Reload(numberOfRoundsAvailable);
        }

        if (Character.disableUserClickingInputStatus)
        {
            return;
        }

        if (Input.GetMouseButton(0))
        {
            // Fully Auto
            if (currentGunHeld != null)
            {
                currentGunHeld.Use();
            }
        }
    }

    private void SwitchGuns()
    {
        if (selectedFirstSlot)
        {
            gearStorageLocations[(int)GearSlotIdentifier.WEAPONSLOT1].transform.SetParent(weaponPositionHands,false);
            gearStorageLocations[(int)GearSlotIdentifier.WEAPONSLOT2].transform.SetParent(weaponPositionHip, false);
            if (gearItems[(int)GearSlotIdentifier.WEAPONSLOT1] != null)
            {
                gearItems[(int)GearSlotIdentifier.WEAPONSLOT1].enabled = true;
            }
            if (gearItems[(int)GearSlotIdentifier.WEAPONSLOT2] != null)
            {
                gearItems[(int)GearSlotIdentifier.WEAPONSLOT2].enabled = false;
            }
            currentGunHeld = (Gun)gearItems[(int)GearSlotIdentifier.WEAPONSLOT1];
        } else {
            gearStorageLocations[(int)GearSlotIdentifier.WEAPONSLOT1].transform.SetParent(weaponPositionHip, false);
            gearStorageLocations[(int)GearSlotIdentifier.WEAPONSLOT2].transform.SetParent(weaponPositionHands, false);
            if (gearItems[(int)GearSlotIdentifier.WEAPONSLOT1] != null)
            {
                gearItems[(int)GearSlotIdentifier.WEAPONSLOT1].enabled = false;
            }
            if (gearItems[(int)GearSlotIdentifier.WEAPONSLOT2] != null)
            {
                gearItems[(int)GearSlotIdentifier.WEAPONSLOT2].enabled = true;
            }
            currentGunHeld = (Gun)gearItems[(int)GearSlotIdentifier.WEAPONSLOT2];
        }
      
    }

    private void GearSlotChange(GearSlot gearSlot)
    {
        if (gearSlot.GetItemType() == ItemType.PRIMARY_WEAPON)
        {
            if (gearSlot == playerInventory.GetGearSlot(GearSlotIdentifier.WEAPONSLOT1))
            {
                HandleGearSlotChange(GearSlotIdentifier.WEAPONSLOT1, gearSlot);
            } else
            {
                HandleGearSlotChange(GearSlotIdentifier.WEAPONSLOT2, gearSlot);
            }
        }
        else if (gearSlot.GetItemType() == ItemType.BACKPACK)
        {
            HandleGearSlotChange(GearSlotIdentifier.BACKPACK, gearSlot);
        }
        else if (gearSlot.GetItemType() == ItemType.HELMET)
        {
            HandleGearSlotChange(GearSlotIdentifier.HELMET, gearSlot);
        }
        else if (gearSlot.GetItemType() == ItemType.ARMOR)
        {
            HandleGearSlotChange(GearSlotIdentifier.ARMOR, gearSlot);
        }
        else if (gearSlot.GetItemType() == ItemType.OTHER)
        {
            Debug.Log("Error: What Kind of Gear is That?");
            //HandleGearSlotChange(GearSlotIdentifier.WEAPONSLOT1, gearSlot);
        }
    }

    private void HandleGearSlotChange(GearSlotIdentifier identifier, GearSlot gearSlot)
    {
        if (gearItems[(int)identifier] != null)
        {
            Destroy(gearItems[(int)identifier].gameObject);
        }
        if (gearSlot.HasItem())
        {
            gearItems[(int)identifier] =
                Instantiate<WorldItem>(gearSlot.GetItemInSlot().item.itemPrefab, gearStorageLocations[(int)identifier]);
            gearItems[(int)identifier].Equip();
            if (selectedFirstSlot)
            {
                if (identifier == GearSlotIdentifier.WEAPONSLOT1)
                {
                    currentGunHeld = (Gun)gearItems[(int)GearSlotIdentifier.WEAPONSLOT1];
                }
            } else
            {
                if (identifier == GearSlotIdentifier.WEAPONSLOT2)
                {
                    currentGunHeld = (Gun)gearItems[(int)GearSlotIdentifier.WEAPONSLOT2];
                }
            }
        }
    }
}
