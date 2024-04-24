using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static PlayerInventory;

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

	public virtual void AddItem(InventorySlot inventorySlot, InventoryItem itemToSet) {
		if (itemToSet == null) {
			return;
		}
		inventorySlot.SetItemInSlotAfterDrag(itemToSet);
		itemToSet.DoThingsAfterMove();
	}

	public virtual bool CanAddItem(InventorySlot inventorySlot, InventoryItem itemToSet) {
		if (inventorySlot.GetInventory() != this) {
			Debug.LogWarning("Developer Must Ensure The Slot is Part of This Inventory");
			return false;
		}
		return true;
	}

	public virtual bool Swap(InventorySlot inventorySlot, InventoryItem itemToSet) {
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

			// Remove Items
			InventoryItem inventoryItemHere = RemoveItemFromSlot(inventorySlot);
			InventoryItem otherItem = otherSlot.GetInventory().RemoveItemFromSlot(otherSlot);

			// Add Items
			AddItem(inventorySlot, itemToSet);
			otherSlot.GetInventory().AddItem(otherSlot, inventoryItemHere);
			return true;
		}

		return false;
	}

	// Returns the number of items left in incoming item
	public int Combine(InventorySlot inventorySlot, InventoryItem itemToCombine) {
		// Assume inventorySlot has an item and these are the same type but confirm
		if (!inventorySlot.HasItem() || itemToCombine == null) {
			return itemToCombine.GetItemCount();
		}
		// If not the same type, return all items
		if (inventorySlot.GetItemInSlot().GetItemType() != itemToCombine.GetItemType()) {
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

	protected InventorySlot FindEarliestEmptySlot(InventoryItem inventoryItem) {
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

	protected InventorySlot FindEarliestEmptySlot() {
		foreach (InventorySlot slot in inventorySlots) {
			if (!slot.HasItem()) {
				return slot; // Return the first empty slot found before the current slot
			}
		}
		return null; // Return null if no suitable slot is found
	}

	public virtual void DropItem(InventorySlot inventorySlot) {

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
}
