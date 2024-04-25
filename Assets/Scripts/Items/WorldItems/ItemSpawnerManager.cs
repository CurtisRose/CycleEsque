using UnityEngine;
using System.Collections.Generic;

public class ItemSpawnerManager : MonoBehaviour
{
	[SerializeField] private List<LootSpawner> spawners = new List<LootSpawner>();
	[SerializeField] private List<LootContainer> lootBoxes = new List<LootContainer>();


	private void Start()
    {
		// Find all ItemSpawner components in the children of this GameObject
		spawners.AddRange(GetComponentsInChildren<LootSpawner>());

		// Find all loot containers in the children of this GameObject
		lootBoxes.AddRange(GetComponentsInChildren<LootContainer>());
	}

    public void Update()
    {
        // Check each spawner if it needs to respawn items
        foreach (LootSpawner spawner in spawners)
        {
            spawner.CheckRespawn();
        }
    }

    // Optionally provide methods to manually add or remove spawners if needed
    public void AddSpawner(LootSpawner spawner)
    {
        if (!spawners.Contains(spawner))
        {
            spawners.Add(spawner);
        }
    }

    public void RemoveSpawner(LootSpawner spawner)
    {
        if (spawners.Contains(spawner))
        {
            spawners.Remove(spawner);
        }
    }
}