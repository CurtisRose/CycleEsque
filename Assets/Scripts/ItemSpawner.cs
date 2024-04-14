using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemPrefabPair
{
    public SharedItemData baseItem;
    public WorldItem prefab;
}

public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner Instance { get; private set; }
    public List<ItemPrefabPair> itemPrefabs = new List<ItemPrefabPair>();
    private Dictionary<SharedItemData, WorldItem> itemPrefabMap;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // Ensures that there aren't multiple spawners.
            return;
        }
        Instance = this;

        itemPrefabMap = new Dictionary<SharedItemData, WorldItem>();
        foreach (ItemPrefabPair pair in itemPrefabs)
        {
            itemPrefabMap[pair.baseItem] = pair.prefab;
        }
    }

    public WorldItem SpawnItem(SharedItemData sharedItemData, Vector3 position, Quaternion rotation)
    {
        if (itemPrefabMap.TryGetValue(sharedItemData, out WorldItem prefab))
        {
            WorldItem newItem = Instantiate(prefab, position, rotation);
            newItem.UpdateBaseItemData(sharedItemData); // Assuming you have this method in WorldItem
            return newItem;
        }
        return null;
    }

    public WorldItem SpawnItem(SharedItemData sharedItemData, Transform parent)
    {
        if (itemPrefabMap.TryGetValue(sharedItemData, out WorldItem prefab))
        {
            WorldItem newItem = Instantiate(prefab, parent);
            newItem.UpdateBaseItemData(sharedItemData); // Assuming you have this method in WorldItem
            return newItem;
        }
        return null;
    }
}