using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGearController : MonoBehaviour
{
    [SerializeField] PlayerInventory playerInventory;
    [SerializeField] bool selectedFirstSlot;

    // GearSlotIdentifier { BACKPACK, ARMOR, HELMET, WEAPONSLOT1, WEAPONSLOT2 }
    [SerializeField] WorldItem[] gearItems;
    [SerializeField] List<Transform> gearStorageLocations;
    [SerializeField] Transform weaponPositionHands;
    [SerializeField] Transform weaponPositionHip;


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
        /*if (scroll != 0f)
        {
            selectedFirstSlot = !selectedFirstSlot;
            SwitchGuns();
        }*/
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
        }
    }
}
