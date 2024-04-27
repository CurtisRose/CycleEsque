using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PointOfInterest : MonoBehaviour, IActivatable
{
	[SerializeField] private List<MonsterSpawner> monsterSpawners;
	[SerializeField] private List<LootSpawner> lootSpawners;
	[SerializeField] private List<LootContainer> lootBoxes;

	public bool isDeactivating = false;
	[SerializeField] private float deactivationDelay = 30.0f;

	bool isActive;

	private void Start()
    {
		// Find all ItemSpawner components in the children of this GameObject
		lootSpawners.AddRange(GetComponentsInChildren<LootSpawner>());

		// Find all loot containers in the children of this GameObject
		lootBoxes.AddRange(GetComponentsInChildren<LootContainer>());
	}
	public void Activate() {
		if (isActive) {
			return;
		}
		if (!isActive || isDeactivating) {
			isActive = true;
			isDeactivating = false;  // Cancel any ongoing deactivation
			StopAllCoroutines();
			Debug.Log("A player has approached the POI, activating POI.");
			foreach (MonsterSpawner monsterSpawner in monsterSpawners) {
				monsterSpawner.Activate();
			}
			foreach (LootSpawner lootSpawner in lootSpawners) {
				lootSpawner.Activate();
			}
			foreach (LootContainer lootContainer in lootBoxes) {
				lootContainer.Activate();
			}
		}
	}

	public void Deactivate() {
		if (isActive && !isDeactivating) {
			StartCoroutine(DeactivateAfterDelay());
		}
	}

	private IEnumerator DeactivateAfterDelay() {
		isDeactivating = true;  // Mark as deactivating
		yield return new WaitForSeconds(deactivationDelay);
		if (isActive) {
			isActive = false;
			isDeactivating = false;  // Reset deactivating state
			Debug.Log("A player has left the POI, deactivating POI.");
			foreach (MonsterSpawner monsterSpawner in monsterSpawners) {
				monsterSpawner.Deactivate();
			}
			foreach (LootSpawner lootSpawner in lootSpawners) {
				lootSpawner.Deactivate();
			}
			foreach (LootContainer lootContainer in lootBoxes) {
				lootContainer.Deactivate();
			}
		} else {
			isDeactivating = false;
		}
	}

	public bool IsActive() {
		return isActive;
	}
}