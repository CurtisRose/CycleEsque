using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGearController : MonoBehaviour
{
    [SerializeField] PlayerInventory playerInventory;
    [SerializeField] bool selectedFirstSlot;
    [SerializeField] public Gun gunInHands = null;
    [SerializeField] public Gun gunOnHip = null;

    // GearSlotIdentifier { BACKPACK, ARMOR, HELMET, WEAPONSLOT1, WEAPONSLOT2 }
    [SerializeField] WorldItem[] gearItems;
    [SerializeField] List<Transform> gearStorageLocations;
    [SerializeField] Transform weaponPositionHands;
    [SerializeField] Transform weaponPositionHip;
    [SerializeField] Transform head;
    [SerializeField] float throwForce;
    [SerializeField] Transform throwPosition;

    // Crosshair Aimer Test
    [SerializeField] CrosshairController crosshairController;
    [SerializeField] float smoothTime = 0.1f; // This should be based on the gun maybe

    public delegate void LoadOutChanged();
    public event LoadOutChanged OnLoadOutChanged;

    public delegate void PrimaryGunFired();
    public event PrimaryGunFired OnPrimaryGunFired;

    public delegate void PrimaryGunReloaded();
    public event PrimaryGunReloaded OnPrimaryGunReloaded;

    public delegate void InventoryChanged();
    public event InventoryChanged OnInventoryChanged;


    private void Awake()
    {
        gearItems = new WorldItem[5];
        foreach(GearSlot gearSlot in playerInventory.GetGearSlots())
        {
            gearSlot.OnGearSlotsChanged += GearSlotChange;
        }
        playerInventory.OnInventoryChanged += OnInventoryChangedPassThrough;
    }

    public void OnInventoryChangedPassThrough()
    {
        if (OnInventoryChanged != null)
        {
            OnInventoryChanged();
        }
    }

    private void Update() 
    {
        if (gunInHands != null)
        {
            crosshairController.SetCrosshairPosition(gunInHands.transform, smoothTime);
        }

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
                InventoryItem inventoryItemBeingDropped = InventoryItem.CurrentHoveredItem;
                inventoryItemBeingDropped.GetCurrentInventorySlot().RemoveItemFromSlot();
                WorldItem itemBeingDropped = Instantiate<WorldItem>(InventoryItem.CurrentHoveredItem.item.itemPrefab, throwPosition.position, Quaternion.identity);
                // Maybe yeet it a little bit
                itemBeingDropped.GetComponent<Rigidbody>().AddForce(head.forward * throwForce * Time.deltaTime, ForceMode.Impulse);
                // This is so the pick up menu doesn't trigger immediately.
                itemBeingDropped.SetUninteractableTemporarily();
                itemBeingDropped.SetNumberOfStartingItems(inventoryItemBeingDropped.GetItemCount());
                Destroy(InventoryItem.CurrentHoveredItem.gameObject);

                if (inventoryItemBeingDropped.item.ItemType == ItemType.PRIMARY_WEAPON)
                {
                    if (OnLoadOutChanged != null)
                    {
                        OnLoadOutChanged();
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (gunInHands == null)
            {
                return;
            }
            // Reload Gun
            int numberOfRoundsAvailable = GetNumberOfRoundsOfAmmoInInventory();
            int numberOfRoundsUsed = gunInHands.Reload(numberOfRoundsAvailable);
            playerInventory.RemoveItemOfType(ItemType.AMMO, numberOfRoundsUsed);
            if (OnPrimaryGunReloaded != null)
            {
                OnPrimaryGunReloaded();
            }
        }

        if (Character.disableUserClickingInputStatus)
        {
            return;
        }

        if (Input.GetMouseButton(0))
        {
            // Fully Auto
            if (gunInHands != null)
            {
                bool fired = gunInHands.Use();
                if (fired)
                {
                    crosshairController.Bloom();
                    if (OnPrimaryGunFired != null)
                    {
                        OnPrimaryGunFired();
                    }
                }
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
            SetCurrentGun((Gun)gearItems[(int)GearSlotIdentifier.WEAPONSLOT1]);
            SetHipGun((Gun)gearItems[(int)GearSlotIdentifier.WEAPONSLOT2]);
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
            SetCurrentGun((Gun)gearItems[(int)GearSlotIdentifier.WEAPONSLOT2]);
            SetHipGun((Gun)gearItems[(int)GearSlotIdentifier.WEAPONSLOT1]);
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
            if (gearItems[(int)identifier] == gunInHands)
            {
                gunInHands = null;
                if (OnLoadOutChanged != null)
                {
                    OnLoadOutChanged();
                }
            } else if(gearItems[(int)identifier] == gunOnHip)
            {
                gunOnHip = null;
                if (OnLoadOutChanged != null)
                {
                    OnLoadOutChanged();
                }
            }
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
                    SetCurrentGun((Gun)gearItems[(int)GearSlotIdentifier.WEAPONSLOT1]);
                } else
                {
                    SetHipGun((Gun)gearItems[(int)GearSlotIdentifier.WEAPONSLOT2]);
                }
            } else
            {
                if (identifier == GearSlotIdentifier.WEAPONSLOT2)
                {
                    SetCurrentGun((Gun)gearItems[(int)GearSlotIdentifier.WEAPONSLOT2]);
                }
                else
                {
                    SetHipGun((Gun)gearItems[(int)GearSlotIdentifier.WEAPONSLOT1]);
                }
            }
        }
    }

    public int GetNumberOfRoundsOfAmmoInInventory()
    {
        return playerInventory.GetNumberOfItemsOfType(ItemType.AMMO);
    }

    private void SetCurrentGun(Gun gun)
    {
        gunInHands = gun;
        if (OnLoadOutChanged != null)
        {
            OnLoadOutChanged();
        }
    }

    private void SetHipGun(Gun gun)
    {
        gunOnHip = gun;
        if (OnLoadOutChanged != null)
        {
            OnLoadOutChanged();
        }
    }
}
