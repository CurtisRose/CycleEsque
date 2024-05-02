using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class StashInventoryManager : Inventory
{
    public static StashInventoryManager Instance;
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
        LoadStashFromJson();
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

    private void OrganizeStash() {
		foreach (InventorySlot thisSlot in inventorySlots) {
			if (thisSlot.HasItem()) {
				ItemInstance itemInstance = thisSlot.GetItemInSlot().itemInstance;
				if (itemInstance.sharedData.Stackable) {
					int numItems = thisSlot.GetItemInSlot().GetItemCount();
					// loop through each slot in the inventory and try to combine them
					foreach (InventorySlot otherSlot in inventorySlots) {
						if (thisSlot == otherSlot) {
							break;
						}
						if (otherSlot.HasItem() && otherSlot.GetItemInSlot().itemInstance.sharedData.ID == itemInstance.sharedData.ID) {
							numItems = Combine(otherSlot, thisSlot.GetItemInSlot());
							if (numItems == 0) {
								break;
							}
						}
					}
				}
			}
		}
	}

    public void SaveStash()
    {
        // Gets all the same stackable items into minimum number of slots
        OrganizeStash();

		List<SerializableItemData> items = new List<SerializableItemData>();
        foreach (InventorySlot inventorySlot in inventorySlots)
        {
            if (inventorySlot.HasItem())
            {
                ItemInstance itemInstance = inventorySlot.GetItemInSlot().itemInstance;
                items.Add(SerializableItemData.FromSharedItemData(itemInstance.sharedData, inventorySlot.GetItemInSlot().GetItemCount()));
            }
        }

        // Sort items by ItemType, then Rarity in reverse, and then DisplayName
        var sortedItems = items.OrderBy(item => item.ItemType)
                               .ThenByDescending(item => item.Rarity) // Reverse order for Rarity
                               .ThenBy(item => item.DisplayName)
                               .ToList();

        string json = JsonUtility.ToJson(new Serialization<List<SerializableItemData>>(sortedItems), true);
        SaveStashToJson(json);
    }


    private void SaveStashToJson(string json)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "stash.json");
        File.WriteAllText(filePath, json);
        Debug.Log($"Saved items to {filePath}");
    }

    public void LoadStashFromJson()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "stash.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            stashSerializableItems = JsonUtility.FromJson<Serialization<List<SerializableItemData>>>(json).Data;
        }
        else
        {
			Debug.LogWarning("No Stash data found. Loading default stash data...");
			TextAsset defaultData = Resources.Load<TextAsset>("NewStash"); // No .json extension needed
			stashSerializableItems = JsonUtility.FromJson<Serialization<List<SerializableItemData>>>(defaultData.text).Data;
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

	public override bool QuickEquip(InventorySlot inventorySlot) {
		PlayerInventory playerInventory = PlayerInventory.Instance;
		InventorySlot emptySlot = playerInventory.FindEarliestEmptySlot();

        bool successfullyAdded = playerInventory.AddItem(emptySlot, inventorySlot.GetItemInSlot());

		// I think this will try to effectively quick sort that item
		if (successfullyAdded) {
			playerInventory.QuickEquip(emptySlot);
			return true;
		}
		return false;
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