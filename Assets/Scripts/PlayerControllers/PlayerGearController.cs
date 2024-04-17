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

    // Recoil
    [SerializeField] Recoil recoil;

    // Aiming Down Sights Positions
    [SerializeField] Transform hipFirePosition;
    [SerializeField] Transform ADSFirePosition;
    [SerializeField] float timeToADS = 0.5f;
    private Coroutine currentTransitionCoroutine;

    // Crosshair Aimer Test
    [SerializeField] CrosshairController crosshairController;
    bool ADSing = false;
    [SerializeField] float smoothTime = 0.1f; // This should be based on the gun maybe
    [SerializeField] float crosshairVisiblityTime = 0.2f;
    bool WeaponAim = true;

    [SerializeField] private float switchCooldown = 0.5f; // Time in seconds between allowed switches
    private float lastSwitchTime = 0;
    private bool switchQueued = false; // Flag to check if a switch has been queued
    private bool nextSelectedFirstSlot; // Stores the intended slot selection after cooldown

    public delegate void LoadOutChanged();
    public event LoadOutChanged OnLoadOutChanged;

    public delegate void PrimaryGunFired();
    public event PrimaryGunFired OnPrimaryGunFired;

    public delegate void PrimaryGunReloaded();
    public event PrimaryGunReloaded OnPrimaryGunReloaded;

    public delegate void InventoryChanged();
    public event InventoryChanged OnInventoryChanged;

    public delegate void PrimaryChanged(GunItem gunData);
    public event PrimaryChanged OnPrimaryChanged;


    private void Awake()
    {
        gearItems = new WorldItem[5];
        foreach(GearSlot gearSlot in playerInventory.GetGearSlots())
        {
            gearSlot.OnGearSlotsChanged += GearSlotChange;
        }
        playerInventory.OnInventoryChanged += OnInventoryChangedPassThrough;
        playerInventory.OnItemDropped += DropItem;
        recoil = GetComponent<Recoil>();
    }

    private void Start()
    {
        
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
        // Set the crosshair to where the gun is pointing
        if (gunInHands != null && WeaponAim)
        {
            crosshairController.SetCrosshairPositionWhereGunIsLooking(gunInHands.transform, smoothTime);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            WeaponAim = !WeaponAim;
            crosshairController.CenterCrosshairOnScreen();
        }

        HandleWeaponSwitchingInput();

        ProcessQueuedWeaponSwitch();

        HandleInventoryItemDropping();

        HandleWeaponReloading();

        HandleWeaponFiring();

        HandleAimingDownSights();
    }

    private void HandleAimingDownSights()
    {
        //hipFirePosition
        //ADSFirePosition
        if (Input.GetMouseButtonDown(1))
        {
            MoveToADS();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            MoveToHipFire();
        }
    }

    private void HandleWeaponFiring()
    {
        if (!Character.disableUserClickingInputStatus)
        {
            if (Input.GetMouseButton(0))
            {
                // Fully Auto
                if (gunInHands != null)
                {
                    bool fired = gunInHands.Use();
                    if (fired)
                    {
                        // Apply Recoil
                        recoil.RecoilFire();

                        if (!ADSing)
                        {
                            ShowCrosshair();
                        }
                        crosshairController.Bloom();

                        // Write decrement of AmmoCount to the inventory slot Item Instance
                        if (selectedFirstSlot)
                        {
                            playerInventory.GetGearSlot(GearSlotIdentifier.WEAPONSLOT1).GetItemInSlot().itemInstance.SetProperty(ItemAttributeKey.AmmoCount, gunInHands.GetNumberOfRounds());
                        } else
                        {
                            playerInventory.GetGearSlot(GearSlotIdentifier.WEAPONSLOT2).GetItemInSlot().itemInstance.SetProperty(ItemAttributeKey.AmmoCount, gunInHands.GetNumberOfRounds());
                        }

                        if (OnPrimaryGunFired != null)
                        {
                            OnPrimaryGunFired();
                        }
                    }
                }
            }
        }
    }

    private void HandleWeaponReloading()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (gunInHands != null)
            {
                // Reload Gun
                int numberOfRoundsAvailable = GetNumberOfRoundsOfAmmoInInventory();
                if (numberOfRoundsAvailable <= 0)
                {
                    return;
                }
                int numberOfRoundsUsed = gunInHands.Reload(numberOfRoundsAvailable);
                if (numberOfRoundsUsed > 0)
                {
                    playerInventory.RemoveItemOfType(ItemType.AMMO, numberOfRoundsUsed);

                    // Write decrement of AmmoCount to the inventory slot Item Instance
                    if (selectedFirstSlot)
                    {
                        playerInventory.GetGearSlot(GearSlotIdentifier.WEAPONSLOT1).GetItemInSlot().itemInstance.SetProperty(ItemAttributeKey.AmmoCount, gunInHands.GetNumberOfRounds());
                    }
                    else
                    {
                        playerInventory.GetGearSlot(GearSlotIdentifier.WEAPONSLOT2).GetItemInSlot().itemInstance.SetProperty(ItemAttributeKey.AmmoCount, gunInHands.GetNumberOfRounds());
                    }

                    if (OnPrimaryGunReloaded != null)
                    {
                        OnPrimaryGunReloaded();
                    }
                }
            }
        }
    }

    private void HandleWeaponSwitchingInput()
    {
        if (Time.time - lastSwitchTime < switchCooldown)
        {
            QueueSwitchIfNeeded();
            return;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        bool key1Pressed = Input.GetKeyDown(KeyCode.Alpha1);
        bool key2Pressed = Input.GetKeyDown(KeyCode.Alpha2);

        if (scroll != 0f)
        {
            selectedFirstSlot = !selectedFirstSlot;
            ExecuteSwitch();
        }
        else if (key1Pressed && !selectedFirstSlot)
        {
            selectedFirstSlot = true;
            ExecuteSwitch();
        }
        else if (key2Pressed && selectedFirstSlot)
        {
            selectedFirstSlot = false;
            ExecuteSwitch();
        }
    }

    private void QueueSwitchIfNeeded()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        bool key1Pressed = Input.GetKeyDown(KeyCode.Alpha1);
        bool key2Pressed = Input.GetKeyDown(KeyCode.Alpha2);

        if (key1Pressed || key2Pressed)
        {
            if (!switchQueued)
            {
                nextSelectedFirstSlot = (scroll != 0f) ? !selectedFirstSlot : key1Pressed;
                switchQueued = true;
            }
        }
    }

    private void ProcessQueuedWeaponSwitch()
    {
        if (switchQueued && Time.time - lastSwitchTime >= switchCooldown)
        {
            selectedFirstSlot = nextSelectedFirstSlot;
            ExecuteSwitch();
            switchQueued = false;
        }
    }

    private void ExecuteSwitch()
    {
        SwitchGuns();
        lastSwitchTime = Time.time;
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
        if (gunInHands != null)
        {
            gunInHands.PlayWeaponSwapSound();
        }
    }

    private void HandleInventoryItemDropping()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (InventoryItem.CurrentHoveredItem != null)
            {
                //InventoryItem.CurrentHoveredItem
                //InventoryItem.CurrentHoveredItem.item
                InventoryItem inventoryItemBeingDropped = InventoryItem.CurrentHoveredItem;
                inventoryItemBeingDropped.GetCurrentInventorySlot().RemoveItemFromSlot();
                DropItem(InventoryItem.CurrentHoveredItem.itemInstance);
                Destroy(InventoryItem.CurrentHoveredItem.gameObject);

                if (inventoryItemBeingDropped.itemInstance.sharedData.ItemType == ItemType.PRIMARY_WEAPON)
                {
                    if (OnLoadOutChanged != null)
                    {
                        OnLoadOutChanged();
                    }
                    if (gunInHands == null)
                    {
                        OnPrimaryChanged((GunItem)inventoryItemBeingDropped.itemInstance.sharedData);
                    }
                }
            }
        }
    }

    private void DropItem(ItemInstance itemInstance)
    {
        WorldItem itemBeingDropped = ItemSpawner.Instance.SpawnItem(itemInstance, throwPosition.position, Quaternion.identity);
        //WorldItem itemBeingDropped = Instantiate<WorldItem>(InventoryItem.CurrentHoveredItem.item.itemPrefab, throwPosition.position, Quaternion.identity);
        // Maybe yeet it a little bit
        itemBeingDropped.InitializeFromItemInstance(itemInstance);
        itemBeingDropped.GetComponent<Rigidbody>().AddForce(head.forward * throwForce * Time.deltaTime, ForceMode.Impulse);
        // This is so the pick up menu doesn't trigger immediately.
        itemBeingDropped.SetUninteractableTemporarily();
        itemBeingDropped.SetNumberOfStartingItems((int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack));
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
            //WorldItem itemBeingDropped = ItemSpawner.Instance.SpawnItem(gearSlot.GetItemInSlot().item, Vector3.zero);
            gearItems[(int)identifier] = ItemSpawner.Instance.SpawnItem(gearSlot.GetItemInSlot().itemInstance, gearStorageLocations[(int)identifier]);
            //gearItems[(int)identifier] = Instantiate<WorldItem>(gearSlot.GetItemInSlot().item.itemPrefab, gearStorageLocations[(int)identifier]);
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
        if (gun != null)
        {
            OnPrimaryChanged(gun.GetGunData());
        }
        if (OnLoadOutChanged != null)
        {
            OnLoadOutChanged();
        }
        if (gunInHands != null)
        {
            gunInHands.SetLayerRecursively(gun.gameObject, LayerMask.NameToLayer("Gun"));
        }
    }

    private void SetHipGun(Gun gun)
    {
        gunOnHip = gun;
        if (OnLoadOutChanged != null)
        {
            OnLoadOutChanged();
        }
        if (gunOnHip != null)
        {
            gunOnHip.SetLayerRecursively(gun.gameObject, LayerMask.NameToLayer("Player"));
        }
    }

    public void MoveToADS()
    {
        if (Character.disableUserClickingInputStatus) return;
        if (gunInHands == null) return;
        StopCoroutine("MoveWeapon");
        currentTransitionCoroutine = StartCoroutine(MoveWeapon(true));
    }

    public void MoveToHipFire()
    {
        StopCoroutine("MoveWeapon");
        currentTransitionCoroutine = StartCoroutine(MoveWeapon(false));
    }

    IEnumerator MoveWeapon(bool toADS)
    {
        if (!toADS)
        {
            ADSing = false;
        }

        if (currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine); // Stop any ongoing transition
        }

        float time = 0;
        Vector3 startLocalPosition = weaponPositionHands.localPosition; // Start from the current local position
        Quaternion startLocalRotation = weaponPositionHands.localRotation; // Start from the current local rotation

        Transform targetTransform = toADS ? ADSFirePosition : hipFirePosition;

        while (time < timeToADS)
        {
            float t = time / timeToADS; // Normalize time
            t = Mathf.SmoothStep(0.0f, 1.0f, t); // Apply SmoothStep for smoother interpolation

            weaponPositionHands.localPosition = Vector3.Lerp(startLocalPosition, targetTransform.localPosition, t);
            weaponPositionHands.localRotation = Quaternion.Lerp(startLocalRotation, targetTransform.localRotation, t);

            time += Time.deltaTime;
            yield return null;
        }

        if (toADS)
        {
            ADSing = true;
            // You are fully ADSed, don't show aiming crosshair
            crosshairController.gameObject.SetActive(false);
            StopCoroutine("ShowCrosshair");
        }
        weaponPositionHands.localPosition = targetTransform.localPosition;
        weaponPositionHands.localRotation = targetTransform.localRotation;
    }

    private void ShowCrosshair()
    {
        if (crosshairController != null)
        {
            StopCoroutine("HideCrosshair");  // Stop the coroutine in case it's already running
            crosshairController.gameObject.SetActive(true);  // Show the crosshair
            StartCoroutine("HideCrosshair");  // Start the coroutine to hide it later
        }
    }

    IEnumerator HideCrosshair()
    {
        yield return new WaitForSeconds(crosshairVisiblityTime);  // Wait for the specified time
        crosshairController.gameObject.SetActive(false);  // Hide the crosshair
    }

    public bool IsADSing()
    {
        return ADSing;
    }


    public Gun GetCurrentGun()
    {
        return gunInHands;
    }
}
