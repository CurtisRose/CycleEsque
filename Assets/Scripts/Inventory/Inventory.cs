using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	public List<InventorySlot> inventorySlots;
	public GameObject inventoryItemPrefab;
	[SerializeField] public static List<Color> rarityColors;

	public virtual bool AddItem(ItemInstance itemInstance) {
		InventorySlot slotToAddTo = FindEarliestEmptySlot();
		if (slotToAddTo == null) {
			Debug.LogWarning("Inventory is full");
			return false;
		}
		CreateNewItem(itemInstance, slotToAddTo);
		return true;
	}

	public virtual bool AddItem(InventoryItem inventoryItem) {
		InventorySlot slotToAddTo = FindEarliestEmptySlot();
		if (slotToAddTo == null) {
			Debug.LogWarning("Inventory is full");
			return false;
		}
		AddItem(slotToAddTo, inventoryItem);
		
		return true;
	}

	public virtual bool AddItem(InventorySlot inventorySlot, InventoryItem itemToSet) {
		if (inventorySlot == null) {
			return false;
		}
		if (itemToSet == null) {
			return false;
		}
		if (inventorySlot.HasItem()) {
			// TODO: Maybe swap, or fill stack
			if (itemToSet.itemInstance.sharedData.Stackable) {
				return Combine(inventorySlot, itemToSet) == 0;
			} else {
				return Swap(inventorySlot, itemToSet);
			}
		}
		if (CanAddItem(inventorySlot, itemToSet)) {
			InventorySlot otherSlot = itemToSet.GetCurrentInventorySlot();

			// If it's coming from another slot, then remove it from that slot
			if (otherSlot != null) {
				Inventory otherInventory = otherSlot.GetInventory();
				otherInventory.RemoveItemFromSlot(otherSlot);
			}
			inventorySlot.SetItemInSlotAfterDrag(itemToSet);
			itemToSet.DoThingsAfterMove();
			return true;
		}
		return false;
	}

	protected virtual bool CanAddItem(InventorySlot inventorySlot, InventoryItem itemToSet) {
		if (inventorySlot.GetInventory() != this) {
			Debug.LogWarning("Developer Must Ensure The Slot is Part of This Inventory");
			return false;
		}
		return true;
	}

	// I can't think of any reason to override this function
	// Even if it were a gear slot, it's not stackable. so... it's fine.
	public virtual void SplitItem(InventoryItem itemToSplit) {
		if (itemToSplit.itemInstance.sharedData.Stackable) {
			int numItemsInStack = itemToSplit.GetItemCount();
			int numItemsToSplit = Mathf.FloorToInt(numItemsInStack / 2);
			if (numItemsToSplit == 0) {
				return;
			}
			InventorySlot currentSlot = itemToSplit.GetCurrentInventorySlot();
			ItemInstance newItemInstance = itemToSplit.itemInstance.Clone();
			newItemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, numItemsToSplit);
			InventoryItem newInventoryItem = CreateInventoryItem(newItemInstance);
			// Removing the item first is key, because adding the item after (for the player inventory)
			// will do a weight check that won't pass otherwise
			RemoveNumItemsFromSlot(currentSlot, numItemsToSplit);
			AddItem(newInventoryItem);
		}
	}

	protected virtual bool Swap(InventorySlot inventorySlot, InventoryItem itemToSet) {
		bool canAddInventory1 = false;
		bool canAddInventory2 = false;
		if (CanAddItem(inventorySlot, itemToSet)) {
			canAddInventory1 = true;
		}
		if (itemToSet.GetCurrentInventorySlot().GetInventory().
			CanAddItem(itemToSet.GetCurrentInventorySlot(), inventorySlot.GetItemInSlot())) {
			canAddInventory2 = true;
		}

		if (canAddInventory1 && canAddInventory2) {
			InventorySlot otherSlot = itemToSet.GetCurrentInventorySlot();

			// Remove Items from slots
			InventoryItem inventoryItemHere = RemoveItemFromSlot(inventorySlot);
			InventoryItem otherItem = otherSlot.GetInventory().RemoveItemFromSlot(otherSlot);

			// Make sure items don't have a reference to the old slot, AddItem cares about this
			if (inventoryItemHere != null) {
				inventoryItemHere.RemoveFromSlot();
			}
			if (otherItem != null) {
				otherItem.RemoveFromSlot(); 
			}

			// Add Items
			AddItem(inventorySlot, itemToSet);
			otherSlot.GetInventory().AddItem(otherSlot, inventoryItemHere);
			return true;
		}

		return false;
	}

	// Returns the number of items left in incoming item
	protected virtual int Combine(InventorySlot inventorySlot, InventoryItem itemToCombine) {
		// Assume inventorySlot has an item and these are the same type but confirm
		if (!inventorySlot.HasItem() || itemToCombine == null) {
			return itemToCombine.GetItemCount();
		}
		// If not the same ID, return all items
		if (inventorySlot.GetItemInSlot().itemInstance.sharedData.ID != itemToCombine.itemInstance.sharedData.ID) {
			return itemToCombine.GetItemCount();
		}
		// If not stackable, return all items
		if (!itemToCombine.itemInstance.sharedData.Stackable) {
			return itemToCombine.GetItemCount();
		}

		int numItemsComingIn = itemToCombine.GetItemCount();
		int numItemsAlreadyInStack = inventorySlot.GetItemInSlot().GetItemCount();
		int maxStackSize = itemToCombine.itemInstance.sharedData.MaxStackSize;
		int numItemsToFill = maxStackSize - numItemsAlreadyInStack;
		int numItemsToTake = Mathf.Min(numItemsComingIn, numItemsToFill);
		itemToCombine.ChangeItemCount(numItemsComingIn - numItemsToTake);
		inventorySlot.GetItemInSlot().AddToItemCount(numItemsToTake);
		if (itemToCombine.GetItemCount() == 0) {
			itemToCombine.GetCurrentInventorySlot().RemoveItemFromSlot();
			itemToCombine.DoThingsAfterMove();
			Destroy(itemToCombine.gameObject);
		}
		return itemToCombine.GetItemCount();
	}

	public virtual InventoryItem RemoveItemFromSlot(InventorySlot inventorySlot) {
		InventoryItem item = inventorySlot.GetItemInSlot();
		inventorySlot.RemoveItemFromSlot();
		return item;
	}

	public InventorySlot FindEarliestEmptySlot(InventoryItem inventoryItem) {
		foreach (InventorySlot slot in inventorySlots) {
			if (slot == inventoryItem.GetCurrentInventorySlot()) {
				break; //  Stop searching when reaching the current slot of the item
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

	// To drop the item from the inventory out into the world
	// This would likely ALSO remove the item, but this is NOT removing the item specifically
	public virtual void DropItem(InventorySlot inventorySlot) {

	}

	public virtual int RemoveNumItemsFromSlot(InventorySlot inventorySlot, int numItems) {
		// If the slot is empty, return false
		if (!inventorySlot.HasItem()) {
			return 0;
		}
		if (numItems == 0) {
			return 0;
		}
		// If the item is stackable, remove the number of items from the stack
		if (inventorySlot.GetItemInSlot().itemInstance.sharedData.Stackable) {
			InventoryItem itemInSlot = inventorySlot.GetItemInSlot();
			if (itemInSlot.GetItemCount() > numItems) {
				itemInSlot.AddToItemCount(-numItems);
			} else if (itemInSlot.GetItemCount() == numItems) {
				inventorySlot.RemoveItemFromSlot();
				Destroy(itemInSlot.gameObject);
			} else {
				inventorySlot.RemoveItemFromSlot();
				Destroy(itemInSlot.gameObject);
				numItems -= itemInSlot.GetItemCount();
			}
		}
		return numItems;
	}

	// This function does add the item to the inventory
	protected virtual void CreateNewItem(ItemInstance itemInstance, InventorySlot inventorySlot) {
		InventoryItem inventoryItem = CreateInventoryItem(itemInstance);
		inventorySlot.SetItemInSlotAfterDrag(inventoryItem);
	}

	// This function does NOT add the item to the inventory
	public InventoryItem CreateInventoryItem(ItemInstance itemInstance) {
		GameObject newItem = Instantiate(inventoryItemPrefab);
		InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
		inventoryItem.name = itemInstance.sharedData.DisplayName;
		inventoryItem.InitializeItem(itemInstance);
		return inventoryItem;
	}

	public virtual bool QuickEquip(InventorySlot inventorySlot) {
		// Quick Equip in a normal inventory moves the item to the earliest empty slot if it isnt stackable
		// if it is stackable, it tries to fill earlier items of the same type, otherwise it moves it to the earliest empty slot
		InventoryItem itemToQuickEquip = inventorySlot.GetItemInSlot();
		if (itemToQuickEquip == null) {
			return false;
		}
		if (!itemToQuickEquip.itemInstance.sharedData.Stackable) {
			InventorySlot earliestEmptySlot = FindEarliestEmptySlot(itemToQuickEquip);
			if (earliestEmptySlot != null) {
				return Swap(earliestEmptySlot, itemToQuickEquip);
			}
		} else {
			// TODO Loop first to find slots that have the same item ID
			// Then loop to find empty slots
			foreach (InventorySlot slot in inventorySlots) {
				if (slot == inventorySlot) {
					break;
				}
				if (slot.HasItem() && slot.GetItemInSlot().itemInstance.sharedData.ID == itemToQuickEquip.itemInstance.sharedData.ID) {
					int remainingItems = Combine(slot, itemToQuickEquip);
					if (remainingItems == 0) {
						// If there are no items left, remove the item and destroy it
						inventorySlot.RemoveItemFromSlot();
						Destroy(itemToQuickEquip.gameObject);
						return true;
					}
				} else if (!slot.HasItem()) {

					itemToQuickEquip.GetCurrentInventorySlot().GetInventory().RemoveItemFromSlot(itemToQuickEquip.GetCurrentInventorySlot());
					AddItem(slot, itemToQuickEquip);
					return true;
				}
			}
		}
		return false;
	}
}
