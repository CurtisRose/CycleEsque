using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] PlayerInventory playerInventory;
    [SerializeField] PlayerGearManager gearManager;
    [SerializeField] PlayerWeaponSwitcher playerWeaponSwitcher;

    [SerializeField] Transform head;
    [SerializeField] float throwForce;
    [SerializeField] Transform throwPosition;

    [SerializeField] Transform weaponPositionHands;

    // Recoil
    [SerializeField] Recoil recoil;

    // Aiming Down Sights Positions
    [SerializeField] Transform hipFirePosition;
    [SerializeField] Transform ADSFirePosition;
    [SerializeField] float timeToADS = 0.3f;
    private Coroutine currentTransitionCoroutine;

    // Crosshair Aimer Test
    [SerializeField] CrosshairController crosshairController;
    bool ADSing = false;
    [SerializeField] float smoothTime = 0.1f; // This should be based on the gun maybe
    [SerializeField] float crosshairVisiblityTime = 0.2f;
    bool WeaponAimTesting = true;

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
        playerInventory.OnInventoryChanged += OnInventoryChangedPassThrough;
        playerInventory.OnItemDropped += DropItem;
        recoil = GetComponent<Recoil>();
        playerWeaponSwitcher = GetComponent<PlayerWeaponSwitcher>();
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
        if (gearManager.GetGunInHands() != null && WeaponAimTesting)
        {
            crosshairController.SetCrosshairPositionWhereGunIsLooking(gearManager.GetGunInHands().transform, smoothTime);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            WeaponAimTesting = !WeaponAimTesting;
            crosshairController.CenterCrosshairOnScreen();
        }

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
                if (gearManager.GetGunInHands())
                {
                    bool fired = gearManager.GetGunInHands().Use();
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
                        if (playerWeaponSwitcher.PrimarySelected())
                        {
                            playerInventory.GetGearSlot(GearSlotIdentifier.WEAPONSLOT1).GetItemInSlot().itemInstance.SetProperty(ItemAttributeKey.AmmoCount, gearManager.GetGunInHands().GetNumberOfRounds());
                        } else
                        {
                            playerInventory.GetGearSlot(GearSlotIdentifier.WEAPONSLOT2).GetItemInSlot().itemInstance.SetProperty(ItemAttributeKey.AmmoCount, gearManager.GetGunInHands().GetNumberOfRounds());
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
            if (gearManager.GetGunInHands() != null)
            {
                // Reload Gun
                int numberOfRoundsAvailable = GetNumberOfRoundsOfAmmoInInventory();
                if (numberOfRoundsAvailable <= 0)
                {
                    return;
                }
                int numberOfRoundsUsed = gearManager.GetGunInHands().Reload(numberOfRoundsAvailable);
                if (numberOfRoundsUsed > 0)
                {
                    playerInventory.RemoveItemOfType(ItemType.AMMO, numberOfRoundsUsed);

                    // Write decrement of AmmoCount to the inventory slot Item Instance
                    if (playerWeaponSwitcher.PrimarySelected())
                    {
                        playerInventory.GetGearSlot(GearSlotIdentifier.WEAPONSLOT1).GetItemInSlot().itemInstance.SetProperty(ItemAttributeKey.AmmoCount, gearManager.GetGunInHands().GetNumberOfRounds());
                    }
                    else
                    {
                        playerInventory.GetGearSlot(GearSlotIdentifier.WEAPONSLOT2).GetItemInSlot().itemInstance.SetProperty(ItemAttributeKey.AmmoCount, gearManager.GetGunInHands().GetNumberOfRounds());
                    }

                    if (OnPrimaryGunReloaded != null)
                    {
                        OnPrimaryGunReloaded();
                    }
                }
            }
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
        itemBeingDropped.GetComponent<Rigidbody>().isKinematic = false;
        itemBeingDropped.GetComponent<Rigidbody>().useGravity = true;
        itemBeingDropped.GetComponent<Rigidbody>().AddForce(head.forward * throwForce, ForceMode.Impulse);
        // This is so the pick up menu doesn't trigger immediately.
        itemBeingDropped.SetUninteractableTemporarily();
        itemBeingDropped.SetNumberOfStartingItems((int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack));
    }

    public int GetNumberOfRoundsOfAmmoInInventory()
    {
        return playerInventory.GetNumberOfItemsOfType(ItemType.AMMO);
    }

    public void MoveToADS()
    {
        if (Character.disableUserClickingInputStatus) return;
        if (gearManager.GetGunInHands() == null) return;
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
}
