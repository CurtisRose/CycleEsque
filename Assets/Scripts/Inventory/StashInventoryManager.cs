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
        if (Input.GetKeyDown(KeyCode.P))
        {
            SaveStash();
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

    public void SaveStash()
    {
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
		bool success = false;

		// Try to 
		if (emptySlot != null) {
			if (playerInventory.CanAddItem(emptySlot, inventorySlot.GetItemInSlot())) {
				if (inventorySlot.GetItemInSlot().itemInstance.sharedData.Stackable) {
					int numItems = inventorySlot.GetItemInSlot().GetItemCount();
					// loop through each slot in the inventory and try to combine them
					foreach (InventorySlot slot in playerInventory.inventorySlots) {
						if (slot.HasItem() && slot.GetItemInSlot().itemInstance.sharedData.ID == inventorySlot.GetItemInSlot().itemInstance.sharedData.ID) {
							numItems = playerInventory.Combine(slot, inventorySlot.GetItemInSlot());
							if (numItems == 0) {
								return true;
							}
						}
					}
				}

				success = playerInventory.Swap(emptySlot, inventorySlot.GetItemInSlot());
				if (success) {
					return true;
				}
			}
            else {
                // Can't add whole stack
                // Try to add as many as possible
                if (inventorySlot.GetItemInSlot().itemInstance.sharedData.Stackable) {
					int numItems = inventorySlot.GetItemInSlot().GetItemCount();
                    float weightLeft = playerInventory.GetInventoryWeightLimit() - playerInventory.GetCurrentInventoryWeight();
					int numItemsToHold = (int)Mathf.Floor(weightLeft / inventorySlot.GetItemInSlot().itemInstance.sharedData.Weight);
					if (numItemsToHold == 0) {
                        return false; // Can't add any
                    }
                    inventorySlot.GetItemInSlot().AddToItemCount(-numItemsToHold);
					
                    // loop through each slot in the inventory and try to combine them
					foreach (InventorySlot slot in playerInventory.inventorySlots) {
						if (slot.HasItem() && slot.GetItemInSlot().itemInstance.sharedData.ID == inventorySlot.GetItemInSlot().itemInstance.sharedData.ID) {
							numItemsToHold = playerInventory.Combine(slot, inventorySlot.GetItemInSlot());
							if (numItemsToHold == 0) {
								return true;
							}
						}
					}

                    // There are items leftover to add, create new instance and add them
                    ItemInstance newItemInstance = new ItemInstance(inventorySlot.GetItemInSlot().itemInstance.sharedData);
                    newItemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, numItemsToHold);
                    playerInventory.AddItem(newItemInstance);
                    return false; // Not all were added
				} else {
                    // Can't add because not stackable
                    return false;
                }
            }
		}

		return base.QuickEquip(inventorySlot);
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