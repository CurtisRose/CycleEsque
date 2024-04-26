using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private List<MonsterController> monsterPrefabs;
    [SerializeField] private Transform spawnPointsParent;
    [SerializeField] private List<Transform> spawnPoints; 
    [SerializeField] private float respawnTime = 5.0f;
	[SerializeField] private float deactivationDelay = 30.0f;  // Time to wait before deactivating
	public bool isDeactivating = false;  // Tracks if the spawner is waiting to deactivate [used only for draw gizmos]

	private Dictionary<MonsterController, MonsterController> activeMonsters = new Dictionary<MonsterController, MonsterController>();
	private bool isActive = false;  // Flag to track whether monsters are spawned

	void Start()
    {
        if (spawnPoints == null || spawnPoints.Count <= 0)
        {
            spawnPoints = new List<Transform>(spawnPointsParent.GetComponentsInChildren<Transform>());
            // Remove the spawnPointParent from the list, since it's included for some reason in the GetComponentsInChildren call
            spawnPoints.RemoveAt(0);
        }

        PointOfInterestManager.Instance.RegisterMonsterSpawner(this);
    }

	public void Activate() {
		if (!isActive || isDeactivating) {
			isActive = true;
			isDeactivating = false;  // Cancel any ongoing deactivation
			StopAllCoroutines();
			Debug.Log("A player has approached the POI, activating monsters.");
			foreach (var monster in monsterPrefabs) {
				SpawnMonster(monster);
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
			Debug.Log("Deactivating monsters after delay.");
			foreach (var monster in activeMonsters.Keys) {
				Destroy(monster.gameObject);
			}
			activeMonsters.Clear();
			isDeactivating = false;  // Reset deactivating state
			isActive = false;
		} else {
			isDeactivating = false;  // Deactivation was cancelled
		}
	}

	private void SpawnMonster(MonsterController prefab)
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points defined for the MonsterSpawner.");
            return;
        }

        // Randomly select a spawn point
        Transform selectedSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        // Generate a random rotation around the y-axis
        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        // Instantiate the monster at the selected spawn point with a random rotation
        MonsterController newMonster = Instantiate(prefab, selectedSpawnPoint.position, randomRotation);
        newMonster.transform.SetParent(transform);  // Optionally set the spawner as the parent
        newMonster.explorationTarget = this.transform;

        newMonster.OnDeath += () => HandleDeath(newMonster, prefab);
        activeMonsters.Add(newMonster, prefab);
    }

    private void HandleDeath(MonsterController monster, MonsterController prefab)
    {
        monster.OnDeath -= () => HandleDeath(monster, prefab);
        if (activeMonsters.ContainsKey(prefab) && activeMonsters[prefab] != null)
        {
            Destroy(activeMonsters[prefab].gameObject, respawnTime);
        }
        StartCoroutine(RespawnMonster(prefab));
    }

    private IEnumerator RespawnMonster(MonsterController prefab)
    {
        yield return new WaitForSeconds(respawnTime);
        SpawnMonster(prefab);
    }

	public bool IsActive() {
		return isActive;
	}

    void OnDrawGizmos() {
		if (IsActive() && !isDeactivating) {
			Gizmos.color = Color.green;  // Active and not deactivating
		} else if (IsActive() && isDeactivating) {
			Gizmos.color = Color.yellow;  // Active but pending deactivation
		} else {
			Gizmos.color = Color.red;  // Fully deactivated
		}

		foreach (Transform spawnPoint in spawnPoints) {
			Gizmos.DrawSphere(spawnPoint.position, 1f);
		}
		Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));
	}
}
