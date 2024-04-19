using UnityEngine;
using System.Collections.Generic;

public class ItemSpawnerManager : MonoBehaviour
{
    private List<ItemSpawner> spawners = new List<ItemSpawner>();

    private void Start()
    {
        // Find all ItemSpawner components in the children of this GameObject
        spawners.AddRange(GetComponentsInChildren<ItemSpawner>());
    }

    public void Update()
    {
        // Check each spawner if it needs to respawn items
        foreach (ItemSpawner spawner in spawners)
        {
            spawner.CheckRespawn();
        }
    }

    // Optionally provide methods to manually add or remove spawners if needed
    public void AddSpawner(ItemSpawner spawner)
    {
        if (!spawners.Contains(spawner))
        {
            spawners.Add(spawner);
        }
    }

    public void RemoveSpawner(ItemSpawner spawner)
    {
        if (spawners.Contains(spawner))
        {
            spawners.Remove(spawner);
        }
    }
}