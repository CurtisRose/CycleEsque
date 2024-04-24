using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquippedItemsMenu : Menu
{
    public static EquippedItemsMenu Instance { get; private set; }

    [SerializeField] PlayerWeaponController playerWeaponController;
    [SerializeField] PlayerWeaponSwitcher playerWeaponSwitcher;
    [SerializeField] PlayerInventory playerInventory;

    [SerializeField] Image weapon1Image;
    [SerializeField] Image weapon1RarityBorder1;
    [SerializeField] Image weapon1RarityBorder2;
    [SerializeField] TMP_Text ammoInMagText;
    [SerializeField] TMP_Text ammoInBackpackText;
    [SerializeField] TMP_Text weapon1NameText;
    [SerializeField] Image backpackIndicatorImage;


    [SerializeField] Image weapon2Image;
    [SerializeField] Image weapon2RarityBorder1;
    [SerializeField] Image weapon2RarityBorder2;
    [SerializeField] TMP_Text weapon2NameText;

    Gun gunHeld;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        playerWeaponSwitcher.OnLoadOutChanged += LoadOutChanged;
        playerWeaponController.OnPrimaryGunFired += UpdateAmmoText;
        playerWeaponController.OnPrimaryGunReloaded += UpdateAmmoText;
        playerInventory.OnInventoryChanged += UpdateAmmoText;
    }

	private void Start() {
        // If this exists, call loadoutchanged to make sure that it picks up the guns equipped and ammo in inventory
		// Although it's not working now, It must be happening after gear initialization
        LoadOutChanged();
	}


	private void UpdateAmmoText()
    {
        if (gunHeld == null)
        {
            return;
        }
        ammoInMagText.text = gunHeld.GetNumberOfRounds().ToString();
        ammoInBackpackText.text = playerWeaponController.GetNumberOfRoundsOfAmmoInInventory().ToString();

    }

    public void LoadOutChanged()
    {
        gunHeld = playerWeaponSwitcher.GetGunInHands();
        if (gunHeld != null)
        {
            weapon1Image.sprite = gunHeld.GetBaseItem().LargeImage;
            weapon1Image.enabled = true;
            ammoInMagText.text = gunHeld.GetNumberOfRounds().ToString();
            ammoInBackpackText.text = playerWeaponController.GetNumberOfRoundsOfAmmoInInventory().ToString();
            weapon1NameText.text = gunHeld.GetBaseItem().name;
            weapon1RarityBorder1.color = RarityColorManager.Instance.GetColorByRarity(gunHeld.GetBaseItem().Rarity);
            weapon1RarityBorder2.color = RarityColorManager.Instance.GetColorByRarity(gunHeld.GetBaseItem().Rarity); ;
            backpackIndicatorImage.enabled = true;
        } else
        {
            weapon1Image.sprite = null;
            weapon1Image.enabled = false;
            ammoInMagText.text = "";
            ammoInBackpackText.text = "";
            weapon1NameText.text = "";
            weapon1RarityBorder1.color = Color.white;
            weapon1RarityBorder2.color = Color.white;
            backpackIndicatorImage.enabled = false;
        }

            Gun gun2 = playerWeaponSwitcher.GetGunOnHip();
        if (gun2 != null)
        {
            weapon2Image.sprite = gun2.GetBaseItem().LargeImage;
            weapon2Image.enabled = true;

            weapon2NameText.text = gun2.GetBaseItem().name;
            weapon2RarityBorder1.color = RarityColorManager.Instance.GetColorByRarity(gun2.GetBaseItem().Rarity);
            weapon2RarityBorder2.color = RarityColorManager.Instance.GetColorByRarity(gun2.GetBaseItem().Rarity); ;
        }
        else
        {
            weapon2Image.sprite = null;
            weapon2Image.enabled = false;
            weapon2NameText.text = "";
            weapon2RarityBorder1.color = Color.white;
            weapon2RarityBorder2.color = Color.white;
        }
    }
}
