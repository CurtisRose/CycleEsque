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

    public WorldItem SpawnItem(ItemInstance itemInstance, Vector3 position, Quaternion rotation)
    {
        if (itemPrefabMap.TryGetValue(itemInstance.sharedData, out WorldItem prefab))
        {
            WorldItem newItem = Instantiate(prefab, position, rotation);
            newItem.InitializeFromItemInstance(itemInstance); // Assuming you have this method in WorldItem
            return newItem;
        }
        return null;
    }

    public WorldItem SpawnItem(ItemInstance itemInstance, Transform parent)
    {
        if (itemPrefabMap.TryGetValue(itemInstance.sharedData, out WorldItem prefab))
        {
            WorldItem newItem = Instantiate(prefab, parent);
            newItem.InitializeFromItemInstance(itemInstance); // Assuming you have this method in WorldItem
            return newItem;
        }
        return null;
    }

    public WorldItem GetPrefab(SharedItemData sharedData)
    {
        return itemPrefabMap[sharedData];
    }
}