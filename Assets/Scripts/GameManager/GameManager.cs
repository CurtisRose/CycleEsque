using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerData initialPlayerData;
    public Dictionary<string, SharedItemData> itemDictionary = new Dictionary<string, SharedItemData>();
    public bool loadPlayer;
    [SerializeField] EquippedItemsMenu equippedItemsMenu;
    [SerializeField] Transform CharacterPrefab;


    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
        LoadAllSharedItemData();
    }

	private void Start() {
		if (loadPlayer) {
			LoadAndInitializePlayer();
            if (equippedItemsMenu != null) {
                equippedItemsMenu.LoadOutChanged();
            }
		}
	}

    public void SavePlayerData()
    {
        try
        {
            PlayerData playerData = new PlayerData(PlayerInventory.Instance);

            // If there's any data that needs to be transformed or sorted before saving
            // For example, if PlayerData contains a List<PlayerAchievements> that needs sorting:
            // playerData.Achievements = playerData.Achievements.OrderBy(ach => ach.Date).ToList();

            string jsonData = JsonUtility.ToJson(playerData, true);  // Consider pretty print for easier debugging

            string filePath = Path.Combine(Application.persistentDataPath, "PlayerData.json");
            File.WriteAllText(filePath, jsonData);

            Debug.Log("Saved Player Data to " + filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to save player data: " + ex.Message);
        }
    }

    private void LoadAndInitializePlayer()
    {
        try
        {
            string filePath = Path.Combine(Application.persistentDataPath, "PlayerData.json");
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                initialPlayerData = PlayerData.FromJson(jsonData);
                InitializePlayer(initialPlayerData);
            }
            else
            {
                Debug.LogWarning("Player data file not found.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to load player data: " + ex.Message);
        }
    }

    void LoadAllSharedItemData()
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

    private void InitializePlayer(PlayerData data)
    {
        Debug.Log("Player initialized with loaded data.");

        // MUST set up gear first, especially in regards to backpacks.... ask me how I know lol
        int weaponCount = 0;
        foreach (SerializableItemData serializableItemData in data.equippedItems)
        {
            if (serializableItemData.ID != "empty")
            {
                if (itemDictionary.TryGetValue(serializableItemData.ID, out SharedItemData itemData))
                {
                    // Create a new ItemInstance with the found SharedItemData
                    ItemInstance newItemInstance = new ItemInstance(itemData);
                    newItemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, 1);

                    GearSlot gearSlot = null;
                    if (serializableItemData.ItemType == ItemType.HELMET)
                    {
                        gearSlot = PlayerInventory.Instance.GetGearSlot(GearSlotIdentifier.HELMET);
                    }
                    else if (serializableItemData.ItemType == ItemType.ARMOR)
                    {
                        gearSlot = PlayerInventory.Instance.GetGearSlot(GearSlotIdentifier.ARMOR);
                    }
                    else if (serializableItemData.ItemType == ItemType.BACKPACK)
                    {
                        gearSlot = PlayerInventory.Instance.GetGearSlot(GearSlotIdentifier.BACKPACK);
                    }
                    else if (serializableItemData.ItemType == ItemType.WEAPON)
                    {
                        if (weaponCount == 0)
                        {
                            gearSlot = PlayerInventory.Instance.GetGearSlot(GearSlotIdentifier.WEAPONSLOT1);
                            weaponCount++;
                        }
                        else
                        {
                            gearSlot = PlayerInventory.Instance.GetGearSlot(GearSlotIdentifier.WEAPONSLOT2);
                        }
                    } else
                    {
                        Debug.LogWarning($"Item with ID {serializableItemData.ID} cannot be equipped in a gear slot.");
                        continue;
                    }

					// Equip the item to the player
					PlayerInventory.Instance.EquipItemInstance(newItemInstance, gearSlot);
                    gearSlot.GetItemInSlot().DoThingsAfterMove();

                    //Debug.Log($"Equipped {newItemInstance.sharedData.DisplayName} to gear slot {serializableItemData.ItemType}");
                }
                else
                {
                    Debug.LogWarning($"Item with ID {serializableItemData.ID} not found in dictionary.");
                }
            }
        }

        // Set up inventory based on the SerializableItemData
        foreach (SerializableItemData serializableItemData in data.inventoryItems)
        {
            if (itemDictionary.TryGetValue(serializableItemData.ID, out SharedItemData itemData))
            {
                // Create a new ItemInstance with the found SharedItemData
                ItemInstance newItemInstance = new ItemInstance(itemData);
                newItemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, serializableItemData.Quantity);

				PlayerInventory.Instance.AddItem(newItemInstance);

                // Here you would add this item instance to the player's inventory
                // For example: playerInventory.AddItem(newItemInstance);
                //Debug.Log($"Added {newItemInstance.sharedData.DisplayName} to inventory with quantity {serializableItemData.Quantity}");
            }
            else
            {
                Debug.LogWarning($"Item with ID {serializableItemData.ID} not found in dictionary.");
            }
        }
    }
}