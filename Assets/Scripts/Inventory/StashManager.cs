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

    // Dictionary to hold the ID to ItemData mapping
    private Dictionary<string, SharedItemData> itemDictionary = new Dictionary<string, SharedItemData>();

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
        LoadAllItems();
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
        string filePath = Path.Combine(Application.persistentDataPath, "items.json");
        File.WriteAllText(filePath, json);
        Debug.Log($"Saved items to {filePath}");
    }

    public void LoadItemsFromJson()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "items.json");
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

    void LoadAllItems()
    {
        // Assuming all items are under the "Resources/Items" directory
        // and further organized by type, e.g., "Weapons", "Armor", etc.
        string[] categories = { "Weapons", "Armor", "Helmets", "Backpacks", "Ammo" };
        foreach (var category in categories)
        {
            SharedItemData[] items = Resources.LoadAll<SharedItemData>($"Items/{category}");
            foreach (SharedItemData item in items)
            {
                if (!itemDictionary.ContainsKey(item.ID))
                {
                    itemDictionary.Add(item.ID, item);
                }
                else
                {
                    Debug.LogWarning($"Duplicate ID found in {category}: {item.ID}");
                }
            }
        }
    }

    public SharedItemData GetItemByID(string id)
{
    if (itemDictionary.TryGetValue(id, out SharedItemData item))
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