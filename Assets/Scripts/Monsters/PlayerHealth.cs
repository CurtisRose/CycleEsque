using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Health
{
	PlayerGearManager gearManager;
		
	protected override void Start() {
		base.Start();
		gearManager = GetComponent<PlayerGearManager>();
	}

	protected override float CalculateDamage(float amount) {
		// For now we'll just do based on armor. Later it should be based on which body part or something like that.
		float armorValue = gearManager.GetArmorValue();
		// each point of armor reduces damage by 10%?
		amount -= amount * (armorValue * 0.1f);

		return base.CalculateDamage(amount);
	}
}
