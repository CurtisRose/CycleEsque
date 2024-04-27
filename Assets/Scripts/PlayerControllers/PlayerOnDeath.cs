using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOnDeath : MonoBehaviour
{
	private void Start() {
		GetComponent<Health>().OnDeath += OnPlayerDeath;
	}

	private void OnPlayerDeath() {
		// Player Died
		PlayerInventory.Instance.DropInventory();
		MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();

		// Add a rigid body so it falls over when it dies
		gameObject.AddComponent<Rigidbody>();

		// Loop through all components and destroy each one
		foreach (MonoBehaviour component in components) {
			// Check if the component is not null to avoid any potential issues
			if (component != null) {
				Destroy(component);
			}
		}
	}
}
