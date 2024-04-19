using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSwitcher : MonoBehaviour
{
    [SerializeField] PlayerGearManager gearManager;
    [SerializeField] Transform weaponPositionHands;
    [SerializeField] Transform weaponPositionHip;

    [SerializeField] Transform weaponSlot1;
    [SerializeField] Transform weaponSlot2;

    [SerializeField] public Gun gunInHands = null;
    [SerializeField] public Gun gunOnHip = null;

    [SerializeField] bool primarySelected = true;

    [SerializeField] private float switchCooldown = 0.5f; // Time in seconds between allowed switches
    private float lastSwitchTime = 0;
    private bool switchQueued = false; // Flag to check if a switch has been queued
    private bool nextSelectedFirstSlot; // Stores the intended slot selection after cooldown

    [SerializeField] float noGunReloadSpeed = 0.5f;

    [SerializeField] bool selectedFirstSlot;

    public delegate void PrimaryChanged(GunSharedItemData gunData);
    public event PrimaryChanged OnPrimaryChanged;

    public delegate void LoadOutChanged();
    public event LoadOutChanged OnLoadOutChanged;

    private void Awake()
    {
        gearManager = GetComponent<PlayerGearManager>();
        gearManager.OnPrimaryChanged += HandleWeaponChangePrimary;
        gearManager.OnSecondaryChanged += HandleWeaponChangeSecondary;
    }

    private void Update()
    {
        HandleWeaponSwitchingInput();
    }

    private void HandleWeaponChangePrimary(Gun gun)
    {
        if (primarySelected)
        {
            HandleEquippingPrimary(gun);
        } else
        {
            HandleEquippingSecondary(gun);
        }
    }
    private void HandleWeaponChangeSecondary(Gun gun)
    {
        if (primarySelected)
        {
            HandleEquippingSecondary(gun);
        }
        else
        {
            HandleEquippingPrimary(gun);
        }
    }

    private void HandleEquippingPrimary(Gun gun)
    {
        gunInHands = gun;
        if (gunInHands != null)
        {
            gunInHands.SetLayerRecursively(gunInHands.gameObject, LayerMask.NameToLayer("Gun"));
            gunInHands.PlayWeaponSwapSound();
            OnPrimaryChanged(gunInHands.GetGunData());
        }
        if (OnLoadOutChanged != null)
        {
            OnLoadOutChanged();
        }
    }

    private void HandleEquippingSecondary(Gun gun)
    {
        gunOnHip = gun;
        if (gunOnHip != null)
        {
            gunOnHip.SetLayerRecursively(gunOnHip.gameObject, LayerMask.NameToLayer("Gun"));
            gunOnHip.PlayWeaponSwapSound();
        }
        if (OnLoadOutChanged != null)
        {
            OnLoadOutChanged();
        }
    }

    private void HandleWeaponSwitchingInput()
    {
        // Check to see if state manager allows this action
        if (!WeaponStateManager.Instance.CanPerformAction(WeaponState.SwappingWeapons)) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        bool key1Pressed = Input.GetKeyDown(KeyCode.Alpha1);
        bool key2Pressed = Input.GetKeyDown(KeyCode.Alpha2);

        bool check = selectedFirstSlot;
        if (scroll != 0f)
        {
            selectedFirstSlot = !selectedFirstSlot;
        }
        else if (key1Pressed && !selectedFirstSlot)
        {
            selectedFirstSlot = true;
        }
        else if (key2Pressed && selectedFirstSlot)
        {
            selectedFirstSlot = false;
        }

        if (check != selectedFirstSlot)
        {
            // Enter the action state
            WeaponStateManager.Instance.EnterState(WeaponState.SwappingWeapons);
            float weaponSwapSpeed = noGunReloadSpeed;
            if (gunOnHip != null)
            {
                weaponSwapSpeed = gunOnHip.GetGunData().reloadTime;
            }
            // Start reloading animation
            // TODO:
            // Exit state after reloading time
            Invoke("ExitSwappingWeaponsState", weaponSwapSpeed);

            // Execute logical switch
            ExecuteSwitch();
        }
    }

    private void ExitSwappingWeaponsState()
    {
        WeaponStateManager.Instance.ExitState(WeaponState.SwappingWeapons);
    }

    private void ExecuteSwitch()
    {
        primarySelected = !primarySelected;
        SwitchGuns();
        lastSwitchTime = Time.time;
    }
    private void SwitchGuns()
    {
        Transform tempParent = weaponSlot1.parent;
        weaponSlot1.SetParent(weaponSlot2.parent, false);
        weaponSlot2.SetParent(tempParent, false);

        gunInHands = weaponPositionHands.GetComponentInChildren<Gun>();
        if (gunInHands != null)
        {
            gunInHands.SetLayerRecursively(gunInHands.gameObject, LayerMask.NameToLayer("Gun"));
            gunInHands.PlayWeaponSwapSound();
            OnPrimaryChanged(gunInHands.GetGunData());
        }
        gunOnHip = weaponPositionHip.GetComponentInChildren<Gun>();
        if (gunOnHip != null)
        {
            gunOnHip.SetLayerRecursively(gunOnHip.gameObject, LayerMask.NameToLayer("Player"));
        }
        if (OnLoadOutChanged != null)
        {
            OnLoadOutChanged();
        }
    }

    public Gun GetGunInHands()
    {
        return gunInHands;
    }

    public Gun GetGunOnHip()
    {
        return gunOnHip;
    }

    public bool PrimarySelected()
    {
        return primarySelected;
    }
}
