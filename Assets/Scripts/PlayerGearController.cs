using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGearController : MonoBehaviour
{
    [SerializeField] PlayerInventory playerInventory;
    [SerializeField] bool selectedFirstSlot;

    private void Awake()
    {
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
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedFirstSlot = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedFirstSlot = false;
        }
    }

    private void GearSlotChange(GearSlot gearSlot)
    {
        if (gearSlot.GetItemType() == ItemType.PRIMARY_WEAPON)
        {
            if (gearSlot == playerInventory.GetGearSlot(GearSlotIdentifier.WEAPONSLOT1))
            {
                //Debug.Log("Weapon Slot 1 Changed");
            } else
            {
                //Debug.Log("Weapon Slot 2 Changed");
            }
        }
        else if (gearSlot.GetItemType() == ItemType.BACKPACK)
        {
            //Debug.Log("Backpack Slot Changed");
        }
        else if (gearSlot.GetItemType() == ItemType.HELMET)
        {
            //Debug.Log("Helmet Slot Changed");
        }
        else if (gearSlot.GetItemType() == ItemType.ARMOR)
        {
            //Debug.Log("Armor Slot Changed");
        }
        else if (gearSlot.GetItemType() == ItemType.OTHER)
        {
            //Debug.Log("Other");
        }
    }
}
