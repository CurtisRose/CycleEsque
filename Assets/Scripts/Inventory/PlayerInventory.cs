using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static GearSlot;

public enum GearSlotIdentifier { BACKPACK, ARMOR, HELMET, WEAPONSLOT1, WEAPONSLOT2 };

[System.Serializable]
public struct InventoryStartItem
{
    public SharedItemData itemData;
    public int quantity;
}

public class PlayerInventory : Inventory, IPlayerInitializable 
{
	public static PlayerInventory Instance;

	[SerializeField] protected float inventoryWeightLimit;
    [SerializeField] protected float currentWeight;

	public GameObject backpackInventory;
	[SerializeField] protected List<GearSlot> gearSlots;
	[SerializeField] protected TMP_Text weightText; // "BACKPACK 0.0/0.0"

	public delegate void InventoryChanged();
	public event InventoryChanged OnInventoryChanged;

	public delegate WorldItem ItemDropped(ItemInstance itemInstance);
	public event ItemDropped OnItemDropped;

	Dictionary<string, List<int>> inventoryDictionary = new Dictionary<string, List<int>>();

	private void Awake() {
		if (Instance != null) {
			Destroy(this);
		} else {
			Instance = this;
		}
	}

	public void Initialize() {
		if (PlayerInventoryMenu.Instance != null) {
			PlayerInventoryMenu.Instance.Open();
		}

		if (PlayerInventoryMenu.Instance != null) {
			PlayerInventoryMenu.Instance.Close();
		}
	}

	protected void UpdateInventoryDictionary() {
		inventoryDictionary.Clear();
		for(int i = 0; i < inventorySlots.Count; i++) {
			if (inventorySlots[i].HasItem()) {
				string itemID = inventorySlots[i].GetItemInSlot().itemInstance.sharedData.ID;
				if (!inventoryDictionary.ContainsKey(itemID)) {
					inventoryDictionary.Add(itemID, new List<int>());
				}
				inventoryDictionary[itemID].Add(i);
			}
		}
		OnInventoryChanged?.Invoke();
	}



	// TODO: This should drop a backpack with weapons and armor and everything else in it
	public void DropInventory() {
		foreach(InventorySlot inventorySlot in inventorySlots) {
			if (inventorySlot.HasItem()) {
				//DropItem(inventorySlot);
				inventorySlot.RemoveItemFromSlot();
			}
		}
		foreach(GearSlot gearSlot in gearSlots) {
			if (gearSlot.HasItem()) {
				//DropItem(gearSlot);
				gearSlot.RemoveItemFromSlot();
			}
		}
	}

	public bool AddItem(WorldItem item) {
		ItemInstance itemInstance = item.CreateItemInstance();
		// Need to do a weight check here
		float weight = itemInstance.sharedData.Weight * (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
		if (currentWeight + weight > GetInventoryWeightLimit()) {
			//return false;
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
			int numberOfItemsStart = item.GetNumberOfItems();
			AddItem(emptySlot, inventoryItem);
			int numberOfItemsAfter = inventoryItem.GetItemCount();
			item.ChangeNumberOfItems(-(numberOfItemsStart - numberOfItemsAfter));
			if (inventoryItem.itemInstance.sharedData.Stackable) {
				QuickEquip(emptySlot);
			}
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
		bool success = base.AddItem(itemInstance);
		if (success) { UpdateInventoryDictionary(); }
        return success;
    }

	public override bool AddItem(InventorySlot inventorySlot, InventoryItem itemToSet) {
		if (inventorySlot == null) {
			return false;
		}
		if (itemToSet == null) {
			return false;
		}
		if (inventorySlot.HasItem()) {
			// TODO: Maybe swap, or fill stack
			return false;
		}

		InventorySlot otherSlot = itemToSet.GetCurrentInventorySlot();

		if (CanAddItem(inventorySlot, itemToSet)) {
			// If it's coming from another slot, then remove it from that slot
			if (otherSlot != null) {
				Inventory otherInventory = otherSlot.GetInventory();
				otherInventory.RemoveItemFromSlot(otherSlot);
			}

			if (inventorySlot is GearSlot) {
				// If adding to a gear slot, don't update the weight
				//UpdateWeight(0);
			} else {
				// If adding to an inventory slot, update the weight
				UpdateWeight(itemToSet.GetTotalWeight());
			}

			inventorySlot.SetItemInSlotAfterDrag(itemToSet);
			itemToSet.DoThingsAfterMove();
			UpdateInventoryDictionary();
			return true;
		} else {
			// CanAddSome?
			float weightLeft = GetInventoryWeightLimit() - GetCurrentInventoryWeight();
			int numItemsToAddByWeight = (int)Mathf.Floor(weightLeft / itemToSet.itemInstance.sharedData.Weight);
			if (numItemsToAddByWeight == 0) {
				return false; // true?
			}
			foreach (int slotIndex in inventoryDictionary[itemToSet.itemInstance.sharedData.ID]) {
				InventorySlot slot = inventorySlots[slotIndex];
				int numItemsInSlot = slot.GetItemInSlot().GetItemCount();
				int numItemsToFill = itemToSet.itemInstance.sharedData.MaxStackSize - numItemsInSlot;
				int numItemsToAddToSlot = Mathf.Min(numItemsToAddByWeight, numItemsToFill);
				
				// If this slot can't add any, continue to the next slot
				if (numItemsToAddToSlot == 0) {
					continue;
				}

				numItemsToAddByWeight -= numItemsToAddToSlot;

				// If it's coming from another slot, then use that inventory to remove the items
				if (otherSlot != null) {
					Inventory otherInventory = otherSlot.GetInventory();
					otherInventory.RemoveNumItemsFromSlot(itemToSet.GetCurrentInventorySlot(), numItemsToAddToSlot);
				} else {
					// If it's not coming from another inventory, just remove the items directly
					itemToSet.AddToItemCount(-numItemsToAddToSlot);
				}

				// Update the weight
				UpdateWeight(numItemsToAddToSlot * itemToSet.itemInstance.sharedData.Weight);

				// Add the correct amount to the next item and update weight
				slot.GetItemInSlot().AddToItemCount(numItemsToAddToSlot);

				if (numItemsToAddByWeight == 0) {
					UpdateInventoryDictionary();
					return false;
				}
			}
			// All other slots are full, add numItemsToAddByWeight to the earliest empty slot
			InventorySlot emptySlot = FindEarliestEmptySlot();
			if (emptySlot != null) {
				// Create new inventoryItem with the remaining items
				InventoryItem inventoryItem = CreateInventoryItem(itemToSet.itemInstance);
				inventoryItem.ChangeItemCount(numItemsToAddByWeight);
				itemToSet.GetCurrentInventorySlot().GetInventory().RemoveNumItemsFromSlot(itemToSet.GetCurrentInventorySlot(), numItemsToAddByWeight);
				AddItem(emptySlot, inventoryItem);
				UpdateInventoryDictionary();
				return false;
			}
		}
		return true;
	}

	protected override bool CanAddItem(InventorySlot inventorySlot, InventoryItem itemToSet) {
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
				// This can happen with guns, probably not worth checking for that though
				return true;
			} else {
				// Inventory to Gear - consider gear slot specifics, typically not changing total weight
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
			// Note for future me, [otherSlot != null] is specifically for swapping guns during quick equip
			if (otherSlot != null && otherSlot.GetInventory() == this) {
				// This should only occur when dragging one gun onto another
				if (otherSlot is GearSlot) {
					// No Weight Change
				}
                else
                {
					// If the item is coming from this inventory, weight must be removed
					weightChange -= newItem.GetTotalWeight();
					if (targetSlot.HasItem()) {
						// If there's an existing item in the slot, add its weight to the change, it's going to the inventory
						weightChange += itemAlreadyHere.GetTotalWeight();
					}
				}
			}
			return weightChange;
		} else {
			// Note for future me, [otherSlot != null] is specifically for when splitting items in the player inventory.
			if (otherSlot != null && otherSlot.GetInventory() == this) {
				// If either slot is a gear slot, there is a weight change in the inventory
				if (otherSlot is GearSlot) {
					// Item is adding weight when moving from gear slot to inventory
					weightChange += newItem.GetTotalWeight();
				}
				if (targetSlot is GearSlot) {
					// Item is reducing weight when moving from inventory to gear slot
					weightChange -= newItem.GetTotalWeight();
				}
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

	protected override bool Swap(InventorySlot slotToAddTo, InventoryItem itemToAdd) {
		bool success = base.Swap(slotToAddTo, itemToAdd);
		if (success) { UpdateInventoryDictionary(); }
		return success;
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

			InventoryItem itemRemoved = base.RemoveItemFromSlot(inventorySlot);
			if (itemRemoved != null) { UpdateInventoryDictionary(); }
			return itemRemoved;
		}
	}

	protected override int Combine(InventorySlot inventorySlot, InventoryItem itemToCombine) {
		int numberOfItemsBeforeCombine = itemToCombine.GetItemCount();
		int numberOfItemsAfterCombine = base.Combine(inventorySlot, itemToCombine);
		if (numberOfItemsBeforeCombine != numberOfItemsAfterCombine) {
			float weightChange = itemToCombine.itemInstance.sharedData.Weight * (numberOfItemsBeforeCombine - numberOfItemsAfterCombine);
			// Only update weight if it's combining from a different inventory.
			if (itemToCombine.GetCurrentInventorySlot().GetInventory() != this) {
				UpdateWeight(weightChange);
				UpdateInventoryDictionary();
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
			UpdateInventoryDictionary();
		}

		return itemsRemoved;
	}

	public bool RemoveItemByID(string itemID, int numItems = 1)
    {
		bool success = false;
		if (inventoryDictionary.ContainsKey(itemID)) {
			foreach (int slotIndex in inventoryDictionary[itemID]) {
				InventoryItem itemInSlot = inventorySlots[slotIndex].GetItemInSlot();
				if (itemInSlot != null && inventorySlots[slotIndex].GetItemInSlot().itemInstance.sharedData.ID == itemID) {
					if (itemInSlot.GetItemCount() > numItems) {
						itemInSlot.AddToItemCount(-numItems);
						UpdateWeight(-(numItems * itemInSlot.itemInstance.sharedData.Weight));
						success = true;
						break;
					} else if (itemInSlot.GetItemCount() == numItems) {
						UpdateWeight(-itemInSlot.GetTotalWeight());
						inventorySlots[slotIndex].RemoveItemFromSlot();
						Destroy(itemInSlot.gameObject);
						success = true;
						break;
					} else {
						UpdateWeight(-itemInSlot.GetTotalWeight());
						inventorySlots[slotIndex].RemoveItemFromSlot();
						Destroy(itemInSlot.gameObject);
						numItems -= itemInSlot.GetItemCount();
					}
				}
			}
		} else {
			return false;
		}

		if (success) { UpdateInventoryDictionary(); }

        return success;
    }

	public float GetCurrentInventoryWeight() {
		return currentWeight;
	}

	public int GetNumberOfItems(string itemID) {
		int numItems = 0;
		if (inventoryDictionary.ContainsKey(itemID)) {
			foreach (int slotIndex in inventoryDictionary[itemID]) {
				if (inventorySlots[slotIndex].HasItem()) {
					numItems += inventorySlots[slotIndex].GetItemInSlot().GetItemCount();
				}
			}
		} else 
		{ 
			return 0; 
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
		UpdateInventoryDictionary();
	}

	// The base quick equip does quick sort... basically.
	public void QuickSort(InventorySlot inventorySlot) {
		base.QuickEquip(inventorySlot);
	}

	public override bool QuickEquip(InventorySlot inventorySlot) {
		InventoryItem inventoryItem = inventorySlot.GetItemInSlot();
		if (inventoryItem == null) {
			return false;
		}

		bool success = false;
		// TODO Should probably be doing weight checks for all of these switches.
		// Determine the slot that is being switched, in the switch statement
		// Then check after the switch statement if the weight is valid
		// Then switch the items if the weight is valid
		switch (inventoryItem.GetItemType()) {
			case ItemType.WEAPON:
				if (inventorySlot is GearSlot) {
					// If the slot clicked is a gear slot, then just swap them
					if (inventorySlot == gearSlots[(int)GearSlotIdentifier.WEAPONSLOT1]) {
						success = Swap(gearSlots[(int)GearSlotIdentifier.WEAPONSLOT2], inventorySlot.GetItemInSlot());
					} else {
						success = Swap(gearSlots[(int)GearSlotIdentifier.WEAPONSLOT1], inventorySlot.GetItemInSlot());
					}
				} else {
					// Weapon needs to take into account weapon slot 1 and 2
					if (gearSlots[(int)GearSlotIdentifier.WEAPONSLOT1].HasItem() && gearSlots[(int)GearSlotIdentifier.WEAPONSLOT2].HasItem()) {
						// If both weapon slots are full, unequip the first weapon to the inventory
						// TODO should probably swap out the gun being held to be consistent
						success = EquipItem(gearSlots[(int)GearSlotIdentifier.WEAPONSLOT1], inventorySlot, false);
					} else if (!gearSlots[(int)GearSlotIdentifier.WEAPONSLOT1].HasItem()) {
						success = EquipItem(gearSlots[(int)GearSlotIdentifier.WEAPONSLOT1], inventorySlot, false);
					} else {
						success = EquipItem(gearSlots[(int)GearSlotIdentifier.WEAPONSLOT2], inventorySlot, false);
					}
				}
				break;
			case ItemType.BACKPACK:
				success = EquipItem(gearSlots[(int)GearSlotIdentifier.BACKPACK], inventorySlot, false);
				break;
			case ItemType.ARMOR:
				success = EquipItem(gearSlots[(int)GearSlotIdentifier.ARMOR], inventorySlot, false);
				break;
			case ItemType.HELMET:
				success = EquipItem(gearSlots[(int)GearSlotIdentifier.HELMET], inventorySlot, false);
				break;
			default:
				success = base.QuickEquip(inventorySlot);
				break;
		}

		return success;
	}

	private bool EquipItem(GearSlot relevantGearSlot, InventorySlot inventorySlotOut, bool  force) {
		// TODO: Add some logic around forced, not forced
		InventorySlot emptySlot = FindEarliestEmptySlot(inventorySlotOut.GetItemInSlot());
		InventoryItem inventoryItem = inventorySlotOut.GetItemInSlot();
		// If the slot in and out are the same, it means they clicked the gear slot just unequip item to earliest empty slot
		if (inventorySlotOut == relevantGearSlot) {
			if (emptySlot != null) {
				if (CanAddItem(emptySlot, inventoryItem)) {
					return Swap(emptySlot, inventoryItem);
				}
			}
		}
		// They want to equip the armor
		else {
			if (CanAddItem(relevantGearSlot, inventoryItem)) {
				return Swap(relevantGearSlot, inventoryItem);
			}
			else {
				if (emptySlot != null) {
					if (CanAddItem(emptySlot, inventoryItem)) {
						return Swap(emptySlot, inventoryItem);
					}
				}
			}
		}
		return false;
	}
}
