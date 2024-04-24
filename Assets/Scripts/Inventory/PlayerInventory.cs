using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum GearSlotIdentifier { BACKPACK, ARMOR, HELMET, WEAPONSLOT1, WEAPONSLOT2 };

[System.Serializable]
public struct InventoryStartItem
{
    public SharedItemData itemData;
    public int quantity;
}

public class PlayerInventory : Inventory {
	public static PlayerInventory Instance;

	[SerializeField] protected float inventoryWeightLimit;
    [SerializeField] protected float currentWeight;

	[SerializeField] protected InventoryStartItem[] startItems;
	public GameObject backpackInventory;
	[SerializeField] protected List<GearSlot> gearSlots;
	[SerializeField] protected TMP_Text weightText; // "BACKPACK 0.0/0.0"

	public delegate void InventoryChanged();
	public event InventoryChanged OnInventoryChanged;

	public delegate void ItemDropped(ItemInstance itemInstance);
	public event ItemDropped OnItemDropped;

	private void Awake() {
		if (Instance != null) {
			Destroy(this);
		} else {
			Instance = this;
		}
	}

	protected void Start() {
		if (PlayerInventoryMenu.Instance != null) {
			PlayerInventoryMenu.Instance.Open();
		}

		foreach (InventoryStartItem startItem in startItems) {
			//ItemInstance itemInstance = new ItemInstance(startItem);
			WorldItem testItem = PlayerItemSpawner.Instance.GetPrefab(startItem.itemData);
			ItemInstance testInstance = testItem.CreateNewItemInstance(startItem.itemData);
			if (startItem.itemData.Stackable) {
				testInstance.SetProperty(ItemAttributeKey.NumItemsInStack, startItem.quantity);
			}

			//itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, 1);
			AddItem(testInstance);
		}

		if (PlayerInventoryMenu.Instance != null) {
			PlayerInventoryMenu.Instance.Close();
		}
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Tab)) {

			if (PlayerInventoryMenu.Instance != null) {
				if (!PlayerInventoryMenu.Instance.IsOpen()) {
					MenuManager.Instance.OpenMenu(PlayerInventoryMenu.Instance);
				} else {
					MenuManager.Instance.CloseMenu(PlayerInventoryMenu.Instance);
				}
			}
		}
	}

	public bool AddItem(WorldItem item) {
		ItemInstance itemInstance = item.CreateItemInstance();
		// Need to do a weight check here
		float weight = itemInstance.sharedData.Weight * (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
		if (currentWeight + weight > GetInventoryWeightLimit()) {
			return false;
		}
		// If the item is stackable, I want to add it to an existing stack if possible
		// First add it to the earliest empty slot then use the QuickEquip method to move it to the correct slot
		InventorySlot emptySlot =  FindEarliestEmptySlot();
		InventoryItem inventoryItem = CreateInventoryItem(itemInstance);
		if (CanAddItem(emptySlot, inventoryItem)) {// Adds to the first empty slot
			// Add the item to the empty slot
			AddItem(emptySlot, inventoryItem);
			if (inventoryItem.itemInstance.sharedData.Stackable) {
				QuickEquip(emptySlot);
			}
			return true;
		} else {
			Destroy(inventoryItem.gameObject);
			return false;
		}
	}

	public override bool AddItem(ItemInstance itemInstance)
    {
		// Need to do a weight check here
		float weight = itemInstance.sharedData.Weight * (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
		if (currentWeight + weight > GetInventoryWeightLimit()) {
			return false;
		}
        return base.AddItem(itemInstance);
    }

	public override void AddItem(InventorySlot inventorySlot, InventoryItem itemToSet) {
		base.AddItem(inventorySlot, itemToSet);
		if (itemToSet == null) {
			return;
		}
		if (inventorySlot is GearSlot) {
			// If adding to a gear slot, don't update the weight
			//UpdateWeight(0);
		} else {
			// If adding to an inventory slot, update the weight
			UpdateWeight(itemToSet.GetTotalWeight());
		}
	}

	public override bool CanAddItem(InventorySlot inventorySlot, InventoryItem itemToSet) {
		// Early exit if base conditions are not met
		if (!base.CanAddItem(inventorySlot, itemToSet)) {
			return false;
		}

		// Calculate weight and capacity changes if necessary
		return CheckWeightAndCapacityChanges(inventorySlot, itemToSet);
	}

	private bool CheckWeightAndCapacityChanges(InventorySlot inventorySlot, InventoryItem itemToSet) {
		float capacityChange = 0;
		// Check if there's no item to set, which might indicate a slot clear or empty operation
		if (itemToSet == null) {
			// If clearing a slot that had a backpack, calculate the reduction in carrying capacity if its coming from the gear slot
			if (inventorySlot.HasItem() && inventorySlot.GetItemInSlot().GetItemType() == ItemType.BACKPACK && inventorySlot is GearSlot) {
				capacityChange = -((BackpackItem)inventorySlot.GetItemInSlot().itemInstance.sharedData).CarryCapacity;
				float test = GetInventoryWeightLimit() + capacityChange;
				bool result = currentWeight <= GetInventoryWeightLimit() + capacityChange;
				return result; // Check if current weight is valid after capacity reduction
			}
			// If it's coming from the back pack and swapping with nothing then we're good
			return true; // No item means no weight change unless it was a backpack
		}

		// Handle weight and capacity based on slot types and item origins
		float weightChange = CalculateWeightChange(itemToSet, inventorySlot);
		capacityChange = CalculateCapacityChange(itemToSet, inventorySlot);

		// Adjust based on where the item is coming from and going to
		if (inventorySlot is GearSlot) {
			if (itemToSet.GetCurrentInventorySlot() is GearSlot) {
				// Gear to Gear - typically blocked or special rules apply
				return false;
			} else {
				// Inventory to Gear - consider gear slot specifics, typically not changing total weight
				float test = GetInventoryWeightLimit() + capacityChange;
				bool result = currentWeight + weightChange <= GetInventoryWeightLimit() + capacityChange;
				return result;
			}
		} else {
			if (itemToSet.GetCurrentInventorySlot() is GearSlot) {
				// Gear to Inventory - adjust weight, reducing if item was in a gear slot
				float test = GetInventoryWeightLimit() + capacityChange;
				bool result = currentWeight + weightChange <= GetInventoryWeightLimit() + capacityChange;
				return result;
			} else {
				// Inventory to Inventory - normal weight check
				float test = GetInventoryWeightLimit();
				bool result = currentWeight + weightChange <= GetInventoryWeightLimit();
				return result;
			}
		}
	}


	private float CalculateWeightChange(InventoryItem newItem, InventorySlot targetSlot) {
		float weightChange = 0;
		InventorySlot otherSlot = newItem.GetCurrentInventorySlot();
		InventoryItem itemAlreadyHere = targetSlot.GetItemInSlot();

		if (targetSlot is GearSlot) {
			if (otherSlot.GetInventory() == this) {
				// If the item is coming from this inventory, weight must be removed
				weightChange -= newItem.GetTotalWeight();
				if (targetSlot.HasItem()) {
					// If there's an existing item in the slot, add its weight to the change, it's going to the inventory
					weightChange += itemAlreadyHere.GetTotalWeight();
				}
			}
			return weightChange;
		} else {
			// Note for future me, [otherSlot != null] is specifically for when splitting items in the player inventory.
			if (otherSlot != null && otherSlot.GetInventory() == this) {
				// The items are just moving in the inventory, no change in weight
				weightChange = 0;
			} else {
				// If the item is coming from another inventory, weight must be added
				weightChange += newItem.GetTotalWeight();
				if (targetSlot.HasItem()) {
					// If there's an existing item in the slot, subtract its weight from the change, it's going to the other inventory
					weightChange -= itemAlreadyHere.GetTotalWeight();
				}
			}	
			return weightChange;
		}
	}

	private float CalculateCapacityChange(InventoryItem newItem, InventorySlot targetSlot) {
		float capacityChange = 0;

		// Check if the new item is a backpack and calculate its capacity contribution.
		if (newItem != null && newItem.GetItemType() == ItemType.BACKPACK) {
			if (targetSlot is GearSlot) {
				capacityChange += ((BackpackItem)newItem.itemInstance.sharedData).CarryCapacity;
			} else {
				capacityChange -= ((BackpackItem)newItem.itemInstance.sharedData).CarryCapacity;
			}
		}

		// Check if there's an existing item in the slot and if it's a backpack.
		if (targetSlot.HasItem()) {
			InventoryItem existingItem = targetSlot.GetItemInSlot();
			if (existingItem.GetItemType() == ItemType.BACKPACK) {
				if (targetSlot is GearSlot) {
					capacityChange -= ((BackpackItem)existingItem.itemInstance.sharedData).CarryCapacity;
				} else {
					capacityChange += ((BackpackItem)existingItem.itemInstance.sharedData).CarryCapacity;
				}
			}
		}

		return capacityChange;
	}

	/*public override bool CanAddItem(InventorySlot inventorySlot, InventoryItem itemToSet) {
		// If base inventory returns false, this should probably return false.
		if (base.CanAddItem(inventorySlot, itemToSet) == false) {
			return false;
		}

		// Items going into or out of gear slots don't add weight to the inventory
		// Backpack items going into or out of backpack gear slots change the inventory weight limit
		// Items going into or out of inventory slots add/remove weight to/from the inventory
		// This class only cares about if it can be added to THIS inventory. The other inventory will handle its own weight changes.
		// Although, if this one is the player inventory, the other one is certainly (As of current design) NOT a player inventory, and therefore not concerned with weight
		// Items can be null, indicating that a swap occurred into an empty slot

		// The item coming from INSIDE the player inventory
		if (itemToSet.GetCurrentInventorySlot().GetInventory() == this) {
			// The item is coming FROM a GEAR slot
			if (itemToSet.GetCurrentInventorySlot() is GearSlot) {
				// The item is going INTO a GEAR slot
				if (inventorySlot is GearSlot) {

				}
				// Is going INTO an INVENTORY slot
				else {
					
				}
			// The item is coming FROM an INVENTORY slot
			} else {
				// The item is going INTO a GEAR slot
				if (inventorySlot is GearSlot) {
					

				}
				// The item is going INTO an INVENTORY slot
				else {
					
				}
			}
		}
		// The item coming from OUTSIDE the player inventory
		else {
			// The item is coming FROM a GEAR slot
			if (itemToSet.GetCurrentInventorySlot() is GearSlot) {
				// The item is going INTO a GEAR slot
				if (inventorySlot is GearSlot) {

				}
				// Is going INTO an INVENTORY slot
				else {
					
				}
			}
			// The item is coming FROM an INVENTORY slot
			else {
				// The item is going INTO a GEAR slot
				if (inventorySlot is GearSlot) {

				}
				// Is going INTO an INVENTORY slot
				else {

				}
			}
		}

		return true;
	}*/

	public override bool Swap(InventorySlot slotToAddTo, InventoryItem itemToAdd) {
		return base.Swap(slotToAddTo, itemToAdd);
	}

	public override InventoryItem RemoveItemFromSlot(InventorySlot inventorySlot) {
		if (inventorySlot.GetItemInSlot() == null) {
			return null;
		}
		else {
			if (inventorySlot is GearSlot) {
				// If removing from a gear slot, don't update the weight
				//UpdateWeight(0);
			} else {
				// If removing from an inventory slot, update the weight
				UpdateWeight(-inventorySlot.GetItemInSlot().GetTotalWeight());
			}
			return base.RemoveItemFromSlot(inventorySlot);
		}
	}

	public override int Combine(InventorySlot inventorySlot, InventoryItem itemToCombine) {
		int numberOfItemsBeforeCombine = itemToCombine.GetItemCount();
		int numberOfItemsAfterCombine = base.Combine(inventorySlot, itemToCombine);
		if (numberOfItemsBeforeCombine != numberOfItemsAfterCombine) {
			float weightChange = itemToCombine.itemInstance.sharedData.Weight * (numberOfItemsBeforeCombine - numberOfItemsAfterCombine);
			// Only update weight if it's combining from a different inventory.
			if (itemToCombine.GetCurrentInventorySlot().GetInventory() != this) {
				UpdateWeight(weightChange);
			}
		}
		return numberOfItemsAfterCombine;
	}

	public override int RemoveNumItemsFromSlot(InventorySlot inventorySlot, int numItems) {
		int itemsRemoved = base.RemoveNumItemsFromSlot(inventorySlot, numItems);
		// Update weight based on differnce between numItems and itemsRemoved
		if (inventorySlot is GearSlot) {
			// If removing from a gear slot, don't update the weight
			//UpdateWeight(0);
		} else {
			// If removing from an inventory slot, update the weight
			UpdateWeight(-itemsRemoved * inventorySlot.GetItemInSlot().itemInstance.sharedData.Weight);
		}

		return itemsRemoved;
	}

	public bool RemoveItemOfType(ItemType type, int numItems = 1)
    {
        foreach (InventorySlot inventorySlot in inventorySlots)
        {
            InventoryItem itemInSlot = inventorySlot.GetItemInSlot();
            if (itemInSlot != null && inventorySlot.GetItemInSlot().GetItemType() == type)
            {
                if (itemInSlot.GetItemCount() > numItems)
                {
                    itemInSlot.AddToItemCount(-numItems);
                    UpdateWeight(-(numItems * itemInSlot.itemInstance.sharedData.Weight));
                    return true;
                } else if (itemInSlot.GetItemCount() == numItems)
                {
                    inventorySlot.RemoveItemFromSlot();
                    Destroy(itemInSlot.gameObject);
                    return true;
                } else
                {
                    inventorySlot.RemoveItemFromSlot();
                    Destroy(itemInSlot.gameObject);
                    numItems -= itemInSlot.GetItemCount();
                }
            }
        }
        return false;
    }

	public float GetCurrentInventoryWeight() {
		return currentWeight;
	}

	public int GetNumberOfItemsOfType(ItemType type) {
		int numItems = 0;
		foreach (InventorySlot inventorySlot in inventorySlots) {
			InventoryItem itemInSlot = inventorySlot.GetItemInSlot();
			if (itemInSlot != null && inventorySlot.GetItemInSlot().GetItemType() == type) {
				if (inventorySlot.GetItemInSlot().itemInstance.sharedData.Stackable) {
					numItems += inventorySlot.GetItemInSlot().GetItemCount();
				} else {
					numItems++;
				}
			}
		}
		return numItems;
	}

	protected override void CreateNewItem(ItemInstance itemInstance, InventorySlot inventorySlot)
    {
        base.CreateNewItem(itemInstance, inventorySlot);
		// Base.CreateNewItem puts the item in sthe inventory slot, but we need to update the weight here
        InventoryItem inventoryItem = inventorySlot.GetItemInSlot();

		// If its not a gear slot, update the weight
		if (!gearSlots.Contains(inventorySlot as GearSlot)) {
			UpdateWeight(inventoryItem.GetTotalWeight());
		}
	}

	public float GetInventoryWeightLimit() {
		if (gearSlots[(int)GearSlotIdentifier.BACKPACK].GetItemInSlot() != null) {
			BackpackItem backpack = (BackpackItem)gearSlots[(int)GearSlotIdentifier.BACKPACK].GetItemInSlot().itemInstance.sharedData;

			return inventoryWeightLimit + backpack.CarryCapacity;
		}
		return inventoryWeightLimit;
	}

	protected void UpdateWeight(float amount) {
        currentWeight += amount;
		UpdateWeightText();
	}

	protected void UpdateWeightText() {
		weightText.text = "BACKPACK " + currentWeight.ToString() + "/" + GetInventoryWeightLimit();
	}

	protected void StartShowSlotAcceptability(InventoryItem inventoryItem) {
		foreach (GearSlot gearSlot in gearSlots) {
			gearSlot.DisplayItemIndication(inventoryItem.GetItemType());
		}
	}

	protected void EndShowSlotAcceptability(InventoryItem inventoryItem) {
		foreach (GearSlot gearSlot in gearSlots) {
			gearSlot.ResetItemIndication();
		}
	}

	public void StartInventoryItemMoved(InventoryItem inventoryItem) {
		StartShowSlotAcceptability(inventoryItem);
	}

	public void EndInventoryItemMoved(InventoryItem inventoryItem) {
		EndShowSlotAcceptability(inventoryItem);
		UpdateWeightText();
	}

	public bool EquipItemInstance(ItemInstance itemInstance, GearSlot gearSlot) {
		ItemType itemType = itemInstance.sharedData.ItemType;
		if (gearSlot.HasItem()) {
			return false;
		} else {
			CreateNewItem(itemInstance, gearSlot);
			return true;
		}

	}

	public List<GearSlot> GetGearSlots() {
		return gearSlots;
	}

	public GearSlot GetGearSlot(GearSlotIdentifier identifier) {
		return gearSlots[(int)identifier];
	}

	public override void DropItem(InventorySlot inventorySlot) {
        InventoryItem inventoryItem = inventorySlot.GetItemInSlot();
        ItemInstance itemInstance = inventoryItem.itemInstance;
        inventorySlot.RemoveItemFromSlot();
		// Update the weight
		UpdateWeight(-inventoryItem.GetTotalWeight());
		if (OnItemDropped != null)
			OnItemDropped(itemInstance);
	}

	void OnValidate() {
		for (int i = 0; i < startItems.Length; i++) {
			if (startItems[i].quantity < 1)
				startItems[i].quantity = 1;
		}
	}
}
