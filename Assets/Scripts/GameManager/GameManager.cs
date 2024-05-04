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
    [SerializeField] Transform CharacterPrefab;


    // Player Menus that need to be initialized
	[SerializeField] PlayerInventory playerInventory;
    [SerializeField] EquippedItemsMenu equippedItemsMenu;
    [SerializeField] PlayerGearUI playerGearUI;
    [SerializeField] ConsumableSelectionMenu consumableSelectionMenu;

	// Player components that need to be initialized
	[SerializeField] PlayerGearManager playerGearManager;
	[SerializeField] PlayerWeaponSwitcher playerWeaponSwitcher;
	[SerializeField] PlayerWeaponController playerWeaponController;

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
            PlayerData playerData = new PlayerData(playerInventory);

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
				Debug.LogWarning("Player data file not found. Loading default data...");
				TextAsset defaultData = Resources.Load<TextAsset>("NewPlayer"); // No .json extension needed
				if (defaultData != null) {
					PlayerData initialPlayerData = PlayerData.FromJson(defaultData.text);
					InitializePlayer(initialPlayerData);
				} else {
					Debug.LogError("Default player data file not found in Resources.");
				}
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
        string[] categories = { "Weapons", "Armor", "Helmets", "Backpacks", "Consumables", "Ammo" };
        foreach (var category in categories)
        {
            SharedItemData[] items = Resources.LoadAll<SharedItemData>($"ItemData/{category}");
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

        // Initialize player components
        if (playerInventory != null) {
            playerInventory.Initialize();
        }

        // This must be done before initializing the playerGearManager.
        // Player Gear Manager equips gear, but playerGearUI subscribes to events from the gearManager
        // PlayerGearUI won't see the changes unless put first.
		if (playerGearUI != null) {
			playerGearUI.Initialize();
		}
		if (playerGearManager != null) {
			playerGearManager.Initialize();
		}
        if (equippedItemsMenu != null) {
			equippedItemsMenu.Initialize();

		}
        if (playerWeaponSwitcher != null) {
            playerWeaponSwitcher.Initialize();
        }
        if (consumableSelectionMenu != null) {
			consumableSelectionMenu.Initialize();
		}
 
        //playerWeaponController.Initialize();

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
                        gearSlot = playerInventory.GetGearSlot(GearSlotIdentifier.HELMET);
                    }
                    else if (serializableItemData.ItemType == ItemType.ARMOR)
                    {
                        gearSlot = playerInventory.GetGearSlot(GearSlotIdentifier.ARMOR);
                    }
                    else if (serializableItemData.ItemType == ItemType.BACKPACK)
                    {
                        gearSlot = playerInventory.GetGearSlot(GearSlotIdentifier.BACKPACK);
                    }
                    else if (serializableItemData.ItemType == ItemType.WEAPON)
                    {
                        if (weaponCount == 0)
                        {
                            gearSlot = playerInventory.GetGearSlot(GearSlotIdentifier.WEAPONSLOT1);
                            weaponCount++;
                        }
                        else
                        {
                            gearSlot = playerInventory.GetGearSlot(GearSlotIdentifier.WEAPONSLOT2);
                        }
                    } else
                    {
                        Debug.LogWarning($"Item with ID {serializableItemData.ID} cannot be equipped in a gear slot.");
                        continue;
                    }

					// Equip the item to the player
					playerInventory.EquipItemInstance(newItemInstance, gearSlot);
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

				playerInventory.AddItem(newItemInstance);

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