using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    PlayerGearManager playerGearManager;
	PlayerWeaponSwitcher playerWeaponSwitcher;
	PlayerWeaponController playerWeaponController;

	public void InitializePlayer(Player player) {
		playerGearManager = player.GetComponent<PlayerGearManager>();
		//playerWeaponSwitcher = player.GetComponent<PlayerWeaponSwitcher>();
		//playerWeaponController = player.GetComponent<PlayerWeaponController>();

		playerGearManager.Initialize();
		//playerWeaponSwitcher.Initialize();
		//playerWeaponController.Initialize();
	}
}
