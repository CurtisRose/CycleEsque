using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour, IActivatable
{
    [SerializeField] private List<MonsterController> monsterPrefabs;
    [SerializeField] private Transform spawnPointsParent;
    [SerializeField] private List<Transform> spawnPoints; 
    [SerializeField] private float respawnTime = 5.0f;
	
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
    }

	public void Activate() {
		if (isActive) {
			return;
		}
		isActive = true;
		foreach (var monster in monsterPrefabs) {
			SpawnMonster(monster);
		}
	}

	public void Deactivate() {
		if (!isActive) {
			return;
		}
		isActive = false;
		foreach (MonsterController monster in activeMonsters.Keys) {
            monster.Destroy();
		}
		activeMonsters.Clear();
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
        activeMonsters.Remove(monster);
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
		if (IsActive()) {
			Gizmos.color = Color.green;  // Active and not deactivating
		} else {
			Gizmos.color = Color.red;  // Fully deactivated
		}

		foreach (Transform spawnPoint in spawnPoints) {
			Gizmos.DrawSphere(spawnPoint.position, 1f);
		}
		Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));
	}
}
