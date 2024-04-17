using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private List<MonsterController> monsterPrefabs;
    [SerializeField] private Transform spawnPointsParent;
    [SerializeField] private List<Transform> spawnPoints; 
    [SerializeField] private float respawnTime = 5.0f; 

    private Dictionary<MonsterController, MonsterController> activeMonsters = new Dictionary<MonsterController, MonsterController>();

    void Start()
    {
        if (spawnPoints == null || spawnPoints.Count <= 0)
        {
            spawnPoints = new List<Transform>(spawnPointsParent.GetComponentsInChildren<Transform>());
            // Remove the spawnPointParent from the list.
            spawnPoints.RemoveAt(0);
        }
        foreach (MonsterController monsterPrefab in monsterPrefabs)
        {
            SpawnMonster(monsterPrefab);
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Transform spawnPoint in spawnPoints)
        {
            Gizmos.DrawSphere(spawnPoint.position, 0.5f);  // Draw a small sphere at each spawn point
        }
    }
}
