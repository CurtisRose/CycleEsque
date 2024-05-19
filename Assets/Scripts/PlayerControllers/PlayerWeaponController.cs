using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponController : MonoBehaviour
{
	public static PlayerWeaponController Instance;

	[SerializeField] PlayerGearManager gearManager;
    [SerializeField] PlayerWeaponSwitcher playerWeaponSwitcher;

    [SerializeField] Transform weaponPositionHands;

    [SerializeField] SharedItemData ammoItemData;

    // Recoil
    [SerializeField] Recoil recoil;

    // Aiming Down Sights Positions
    [SerializeField] Transform hipFirePosition;
    [SerializeField] Transform ADSFirePosition;
    [SerializeField] float timeToADS = 0.3f;
    private Coroutine currentTransitionCoroutine;

    // Crosshair Aimer Test
    CrosshairController crosshairController;
    bool ADSing = false;
    [SerializeField] float smoothTime = 0.1f; // This should be based on the gun maybe
    [SerializeField] float crosshairVisiblityTime = 0.2f;
    bool WeaponAimTesting = true;

    public delegate void PrimaryGunFired();
    public event PrimaryGunFired OnPrimaryGunFired;

    public delegate void PrimaryGunReloaded();
    public event PrimaryGunReloaded OnPrimaryGunReloaded;

    bool isADSing = false;

    private void Awake()
    {
		if (Instance != null) {
			Destroy(this);
		} else {
			Instance = this;
		}
		recoil = GetComponent<Recoil>();
        playerWeaponSwitcher = GetComponent<PlayerWeaponSwitcher>();
    }

	private void Start() {
		crosshairController = CrosshairController.Instance;
	}

	private void Update()
    {
        // Set the crosshair to where the gun is pointing
        if (gearManager.GetGunInHands() != null && WeaponAimTesting && !IsADSing())
        {
            // TODO: This is occuring every frame, even when not aiming or shooting.
            crosshairController.SetCrosshairPositionWhereGunIsLooking(gearManager.GetGunInHands().transform, smoothTime);
        } else
        {
            crosshairController.CenterCrosshairOnScreen();
        }

        //HandleInventoryItemDropping();

        HandleWeaponReloading();

        HandleWeaponFiring();

        HandleAimingDownSights();
    }

    private void HandleAimingDownSights()
    {
        //hipFirePosition
        //ADSFirePosition
        if (Input.GetMouseButton(1))
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
        if (!Player.disableUserClickingInputStatus)
        {
            if (Input.GetMouseButton(0))
            {
                // Check to see if state manager allows this action
                if (!ActionStateManager.Instance.CanPerformAction(ActionState.Firing)) return;

                // Fully Auto
                if (gearManager.GetGunInHands())
                {
                    bool fired = gearManager.GetGunInHands().Use();
                    if (fired)
                    {
                        // Enter the action state
                        ActionStateManager.Instance.TrySetShooting(true);

						PlayerSoundController.Instance.RegisterSound(PlayerNoiseLevel.VeryHigh, transform.position);

						// Start shooting animation
						// TODO: bullet casings, whatever
						// Exit state after rate of fire time
						Invoke("ExitShootingWeaponState", gearManager.GetGunInHands().GetGunData().RateOfFire);

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
							PlayerInventory.Instance.GetGearSlot(GearSlotIdentifier.WEAPONSLOT1).GetItemInSlot().itemInstance.SetProperty(ItemAttributeKey.AmmoCount, gearManager.GetGunInHands().GetNumberOfRounds());
                        } else
                        {
							PlayerInventory.Instance.GetGearSlot(GearSlotIdentifier.WEAPONSLOT2).GetItemInSlot().itemInstance.SetProperty(ItemAttributeKey.AmmoCount, gearManager.GetGunInHands().GetNumberOfRounds());
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

    private void ExitShootingWeaponState()
    {
        ActionStateManager.Instance.TrySetShooting(false);
    }

    private void HandleWeaponReloading()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (gearManager.GetGunInHands() != null)
            {
                // Check to see if state manager allows this action
                if (!ActionStateManager.Instance.CanPerformAction(ActionState.Reloading)) return;

                // Reload Gun
                int numberOfRoundsAvailable = GetNumberOfRoundsOfAmmoInInventory();
                if (numberOfRoundsAvailable <= 0)
                {
                    return;
                }
                // If gun is already full, don't reload
                if (gearManager.GetGunInHands().GetNumberOfRounds() == gearManager.GetGunInHands().GetGunData().MagazineCapacity) {
					return;
				}

                // Enter the action state
                ActionStateManager.Instance.TrySetReloading(true);

                PlayerSoundController.Instance.RegisterSound(PlayerNoiseLevel.Medium, transform.position);

                // Exit state after reloading time
                Invoke("ExitReloadingWeaponsState", gearManager.GetGunInHands().GetGunData().reloadTime);
            }
        }
    }

    private void ExitReloadingWeaponsState()
    {
		int numberOfRoundsAvailable = GetNumberOfRoundsOfAmmoInInventory();
		int numberOfRoundsUsed = gearManager.GetGunInHands().Reload(numberOfRoundsAvailable);
		if (numberOfRoundsUsed > 0) {
			PlayerInventory.Instance.RemoveItemByID(ammoItemData.ID, numberOfRoundsUsed);

			// Write decrement of AmmoCount to the inventory slot Item Instance
			if (playerWeaponSwitcher.PrimarySelected()) {
				PlayerInventory.Instance.GetGearSlot(GearSlotIdentifier.WEAPONSLOT1).GetItemInSlot().itemInstance.SetProperty(ItemAttributeKey.AmmoCount, gearManager.GetGunInHands().GetNumberOfRounds());
			} else {
				PlayerInventory.Instance.GetGearSlot(GearSlotIdentifier.WEAPONSLOT2).GetItemInSlot().itemInstance.SetProperty(ItemAttributeKey.AmmoCount, gearManager.GetGunInHands().GetNumberOfRounds());
			}

			if (OnPrimaryGunReloaded != null) {
				OnPrimaryGunReloaded();
			}
		}
		ActionStateManager.Instance.TrySetReloading(false);
	}

    public int GetNumberOfRoundsOfAmmoInInventory()
    {
        // TODO: This should be based on the gun type
        return PlayerInventory.Instance.GetNumberOfItems(ammoItemData.ID);
    }

    public void MoveToADS()
    {
        if (ActionStateManager.Instance.IsAiming) return;
        if (Player.disableUserClickingInputStatus) return;
        if (gearManager.GetGunInHands() == null) return;

		// Check to see if state manager allows this action
		if (!ActionStateManager.Instance.CanPerformAction(ActionState.Aiming)) return;
        ActionStateManager.Instance.TrySetAiming(true);
		PlayerSoundController.Instance.RegisterSound(PlayerNoiseLevel.Low, transform.position);

		StopCoroutine("MoveWeapon");
        currentTransitionCoroutine = StartCoroutine(MoveWeapon(true));
    }

    public void MoveToHipFire()
    {
        StopCoroutine("MoveWeapon");
		ActionStateManager.Instance.TrySetAiming(false);
		//PlayerSoundController.Instance.RegisterSound(PlayerNoiseLevel.Low, transform.position);
		currentTransitionCoroutine = StartCoroutine(MoveWeapon(false));
	}

    IEnumerator MoveWeapon(bool toADS)
    {
        if (!toADS)
        {
            ADSing = false;
            //AnimationManager.Instance.HandleAnimationCommand(AnimationCommand.Idle);
        } else {
			//AnimationManager.Instance.HandleAnimationCommand(AnimationCommand.Aim);
		}

        if (currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine); // Stop any ongoing transition
        }

        float time = 0;
        

        while (time < timeToADS)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (toADS)
        {
            ADSing = true;
            // You are fully ADSed, don't show aiming crosshair
            crosshairController.SetCrossHairVisual(false);
            StopCoroutine("ShowCrosshair");
        }
    }

    private void ShowCrosshair()
    {
        if (crosshairController != null)
        {
            StopCoroutine("HideCrosshair");  // Stop the coroutine in case it's already running
            crosshairController.SetCrossHairVisual(true);  // Show the crosshair
            StartCoroutine("HideCrosshair");  // Start the coroutine to hide it later
        }
    }

    IEnumerator HideCrosshair()
    {
        yield return new WaitForSeconds(crosshairVisiblityTime);  // Wait for the specified time
        crosshairController.SetCrossHairVisual(false);  // Hide the crosshair
    }

    public bool IsADSing()
    {
        return ADSing;
    }
}
