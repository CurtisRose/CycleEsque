using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class StashManager : Inventory
{
    public static StashManager Instance;
    private static string filePath;
    [SerializeField] GameObject menuContainer;
    [SerializeField] List<SerializableItemData> stashSerializableItems;

    private void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "stash.json");
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        MenuManager.Instance.OpenMenu(StashMenu.Instance);
        LoadItemsFromJson();
        PopulateStash();
        MenuManager.Instance.CloseMenu(StashMenu.Instance);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (StashMenu.Instance.IsOpen())
            {
                MenuManager.Instance.CloseMenu(StashMenu.Instance);
            }
            else
            {
                MenuManager.Instance.OpenMenu(StashMenu.Instance);
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SaveStashToJson();
        }
        if(Input.GetKeyDown(KeyCode.O))
        {
            SaveStashToJson();
        }
    }

    private void PopulateStash()
    {
        foreach(SerializableItemData itemData in stashSerializableItems)
        {
            // Get the data out
            SharedItemData item = GetItemByID(itemData.ID);
            int quantity = itemData.Quantity;

            // Create Item Instance and set Quantity
            ItemInstance itemInstance = new ItemInstance(item);
            itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, quantity);

            // Add it to the inventory
            AddItem(itemInstance);
        }
    }

    public void SaveStashToJson()
    {
        List<SerializableItemData> items = new List<SerializableItemData>();
        foreach (InventorySlot inventorySlot in inventorySlots)
        {
            if (inventorySlot.HasItem())
            {
                ItemInstance itemInstance = inventorySlot.GetItemInSlot().itemInstance;
                items.Add(SerializableItemData.FromSharedItemData(itemInstance.sharedData, (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack)));
            }
        }

        // Sort items by ItemType, then Rarity in reverse, and then DisplayName
        var sortedItems = items.OrderBy(item => item.ItemType)
                               .ThenByDescending(item => item.Rarity) // Reverse order for Rarity
                               .ThenBy(item => item.DisplayName)
                               .ToList();

        string json = JsonUtility.ToJson(new Serialization<List<SerializableItemData>>(sortedItems), true);
        WriteToJsonFile(json);
    }


    private void WriteToJsonFile(string json)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "stash.json");
        File.WriteAllText(filePath, json);
        Debug.Log($"Saved items to {filePath}");
    }

    public void LoadItemsFromJson()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "stash.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            stashSerializableItems = JsonUtility.FromJson<Serialization<List<SerializableItemData>>>(json).Data;
        }
        else
        {
            Debug.LogWarning("File not found, cannot load items.");
        }
    }

    public SharedItemData GetItemByID(string id)
{
    if (GameManager.Instance.itemDictionary.TryGetValue(id, out SharedItemData item))
    {
        return item;
    }
    Debug.LogWarning($"Item with ID {id} not found.");
    return null;
}
}

[System.Serializable]
public class Serialization<T>
{
    public T Data;

    public Serialization(T data)
    {
        this.Data = data;
    }
}