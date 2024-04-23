using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerInventory;

public class Inventory : MonoBehaviour
{
	public List<InventorySlot> inventorySlots;
	public GameObject inventoryItemPrefab;
	[SerializeField] public static List<Color> rarityColors;


	public virtual bool AddItem(ItemInstance itemInstance) {
		int numItems = HowManyItemsCanBeAdded(itemInstance);

		if (numItems <= 0) {
			Debug.Log("No Room In Inventory");
			return false;
		}

		bool updated = false; // Flag to track if the inventory was updated

		// If the item is stackable, try to add it to existing slots with the same item
		if (itemInstance.sharedData.Stackable) {
			int startItems = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
			int remainingItems = FillExistingStacks(itemInstance);
			int itemsAdded = startItems - remainingItems;
			if (remainingItems < numItems) {
				updated = true;  // Update occurred if we added some items to existing stacks
			}
			numItems = remainingItems;
		}

		// If there are items left after trying to stack them in existing slots, or if the item is not stackable
		if (numItems > 0) {
			int itemsLeftToAdd = FillEmptySlots(itemInstance);
			if (itemsLeftToAdd < numItems) {
				updated = true;  // Update occurred if we added some items to new slots
			}
		}

		return updated;
	}

	public virtual bool AddItem(InventorySlot slotToAddTo, InventoryItem itemToAdd) {
		
		// Drastic, if the other one is a player inventory, just let it do the adding
		if (itemToAdd.GetCurrentInventorySlot().GetInventory() as PlayerInventory) {
			return ((PlayerInventory)itemToAdd.GetCurrentInventorySlot().GetInventory()).AddItem(slotToAddTo, itemToAdd);
		}

		// If either slot is a gear slot, make sure both are allowed to fit the item that's being swapped
		if (slotToAddTo as GearSlot) {
			if (((GearSlot)slotToAddTo).GetItemType() != itemToAdd.GetItemType()) {
				return false;
			}
		}

		// If itemslot has item, swap, unless stackable and same item type
		if (slotToAddTo.HasItem()) {
			Swap(slotToAddTo, itemToAdd);
		} else {
			PlaceItem(itemToAdd, slotToAddTo);
		}

		return true;
	}

	public void PlaceItem(InventoryItem item, InventorySlot inventorySlot) {
		if (item != null) {

			Swap(inventorySlot, item);
		}
	}

	public virtual void QuickEquip(InventorySlot inventorySlotToAddFrom) {
		InventoryItem inventoryItemToAdd = inventorySlotToAddFrom.GetItemInSlot();
		if (inventoryItemToAdd.itemInstance.sharedData.Stackable) {
			int itemsToAdd = inventoryItemToAdd.GetItemCount();
			foreach (InventorySlot inventorySlot in inventorySlots) {
				InventoryItem potentialSlotItem = inventorySlot.GetItemInSlot();
				if (potentialSlotItem != null && potentialSlotItem.GetItemType() == inventoryItemToAdd.GetItemType()) {
					int numItems = potentialSlotItem.GetItemCount();
					int spaceLeftInSlot = potentialSlotItem.itemInstance.sharedData.MaxStackSize - numItems;
					if (spaceLeftInSlot > 0) {
						int itemsToAddToSlot = Mathf.Min(spaceLeftInSlot, itemsToAdd);
						potentialSlotItem.AddToItemCount(itemsToAddToSlot);
						inventoryItemToAdd.itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, itemsToAdd - itemsToAddToSlot);
						itemsToAdd -= itemsToAddToSlot;
						if (itemsToAdd == 0) {
							break;
						}
					}
				}
			}
			// If there are still items left, add them to the earliest empty slot
			if (itemsToAdd > 0) {
				AddItemToEarliestEmptySlot(inventoryItemToAdd);
			}
		} else {
			AddItemToEarliestEmptySlot(inventoryItemToAdd);
		}
	}

	public virtual void SplitInventoryItem(InventoryItem inventoryItem) {
		if (inventoryItem.itemInstance.sharedData.Stackable) {
			if (inventoryItem.GetItemCount() <= 1) {
				return;
			}
			InventorySlot otherSlot = FindEarliestEmptySlot();
			// If there isn't another empty slot, then out of luck.
			if (otherSlot == null) {
				return;
			}
			
			int numItems = inventoryItem.GetItemCount();
			int newStackNum = Mathf.FloorToInt(numItems / 2);

			// Remove the correct number of items from the existing property, 
			inventoryItem.itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, numItems - newStackNum);

			// Create new itemInstance, set it's number, fill empty slot with it.
			ItemInstance newItem = new ItemInstance(inventoryItem.itemInstance.sharedData);
			newItem.SetProperty(ItemAttributeKey.NumItemsInStack, newStackNum);
			FillEmptySlots(newItem);
		}
	}

	public void AddItemToEarliestEmptySlot(InventoryItem inventoryItem) {
		InventorySlot emptySlot = FindEarliestEmptySlot(inventoryItem);
		if (emptySlot != null) {
			Swap(emptySlot, inventoryItem);
		} else {
			//Debug.Log("No earlier empty slot available.");
		}
	}

	public virtual void Swap(InventorySlot inventorySlot, InventoryItem incomingItem) {
		if (inventorySlot.HasItem()) {
			InventoryItem inventoryItemAlreadyHere = inventorySlot.GetItemInSlot();
			InventorySlot otherSlot = incomingItem.GetCurrentInventorySlot();
			otherSlot.RemoveItemFromSlot();
			inventorySlot.RemoveItemFromSlot();

			otherSlot.SetItemInSlotAfterDrag(inventoryItemAlreadyHere);
			inventorySlot.SetItemInSlotAfterDrag(incomingItem);

			if (otherSlot.ContributesToWeight()) {
				if (otherSlot.GetInventory() as PlayerInventory) {
					((PlayerInventory)otherSlot.GetInventory()).UpdateWeight(inventoryItemAlreadyHere.GetTotalWeight());
					((PlayerInventory)otherSlot.GetInventory()).UpdateWeight(-incomingItem.GetTotalWeight());
				}
			}
			if (inventorySlot.ContributesToWeight()) {
				((PlayerInventory)this).UpdateWeight(incomingItem.GetTotalWeight());
				((PlayerInventory)this).UpdateWeight(-inventoryItemAlreadyHere.GetTotalWeight());
			}

			inventoryItemAlreadyHere.DoThingsAfterMove();
			incomingItem.DoThingsAfterMove();
		} else {
			InventorySlot otherSlot = incomingItem.GetCurrentInventorySlot();
			otherSlot.RemoveItemFromSlot();
			if (otherSlot.ContributesToWeight()) {
				if (otherSlot.GetInventory() as PlayerInventory) {
					((PlayerInventory)otherSlot.GetInventory()).UpdateWeight(-incomingItem.GetTotalWeight());

				}
			}
			inventorySlot.SetItemInSlotAfterDrag(incomingItem);
			if (inventorySlot.ContributesToWeight()) {
				if (inventorySlot.GetInventory() as PlayerInventory) {
					((PlayerInventory)inventorySlot.GetInventory()).UpdateWeight(incomingItem.GetTotalWeight());
				}
			}
			incomingItem.DoThingsAfterMove();
		}
	}

	public InventorySlot FindEarliestEmptySlot(InventoryItem inventoryItem) {
		foreach (InventorySlot slot in inventorySlots) {
			if (slot == inventoryItem.GetCurrentInventorySlot()) {
				break; // Stop searching when reaching the current slot of the item
			}
			if (!slot.HasItem()) {
				return slot; // Return the first empty slot found before the current slot
			}
		}
		return null; // Return null if no suitable slot is found
	}

	public InventorySlot FindEarliestEmptySlot() {
		foreach (InventorySlot slot in inventorySlots) {
			if (!slot.HasItem()) {
				return slot; // Return the first empty slot found before the current slot
			}
		}
		return null; // Return null if no suitable slot is found
	}

	public virtual void DropItem(InventorySlot inventorySlot) {

	}

	public virtual int HowManyItemsCanBeAdded(ItemInstance itemInstance) {
		int numItems = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);

		return numItems;
	}

	protected virtual int FillExistingStacks(ItemInstance itemInstance) {
		int numItems = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
		foreach (InventorySlot slot in inventorySlots) {
			InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
			if (itemInSlot != null && itemInSlot.itemInstance.sharedData == itemInstance.sharedData) {
				int spaceLeftInSlot = itemInSlot.itemInstance.sharedData.MaxStackSize - itemInSlot.GetItemCount();
				if (spaceLeftInSlot > 0) {
					int itemsToAdd = Mathf.Min(spaceLeftInSlot, numItems);
					if (itemInstance.sharedData.Stackable) {
						itemInSlot.AddToItemCount(itemsToAdd);
						numItems -= itemsToAdd;
						itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, numItems);
					}
					if (numItems == 0) break; // All items added, exit loop
				}
			}
		}
		return numItems; // Return the number of items left to add
	}

	public virtual int FillEmptySlots(ItemInstance itemInstance) {
		int itemsToAdd = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);

		if (itemsToAdd <= 0) {
			Debug.Log("No items in the item instance");
			return itemsToAdd; 
		}

		foreach (InventorySlot slot in inventorySlots) {
			if (!slot.HasItem() && itemsToAdd > 0) {
				int itemsThatCanBeAdded = itemInstance.sharedData.Stackable ? Mathf.Min(itemInstance.sharedData.MaxStackSize, itemsToAdd) : 1;
				ItemInstance newItemInstance = itemInstance.Clone(); // Create a deep copy of itemInstance
				newItemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, itemsThatCanBeAdded);

				CreateNewItem(newItemInstance, slot);  // Create a new item in the slot

				itemsToAdd -= itemsThatCanBeAdded; // Reduce the number of items left to add
				if (itemsToAdd == 0) return 0; // Successfully added all items
			}
		}

		if (itemsToAdd > 0) {
			Debug.Log("Inventory is full or not enough slots to accommodate all items.");
		}

		return itemsToAdd; // Return the number of items left that could not be added
	}

	protected virtual void CreateNewItem(ItemInstance itemInstance, InventorySlot inventorySlot) {
		GameObject newItem = Instantiate(inventoryItemPrefab, inventorySlot.itemSlot);
		InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
		inventoryItem.name = itemInstance.sharedData.DisplayName;
		inventoryItem.InitializeItem(itemInstance);
		inventorySlot.SetItemInSlotAfterDrag(inventoryItem);
	}
}
