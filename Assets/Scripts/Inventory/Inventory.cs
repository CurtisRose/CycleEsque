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

	protected virtual void CreateNewItem(ItemInstance itemInstance, InventorySlot inventorySlot) {
		GameObject newItem = Instantiate(inventoryItemPrefab, inventorySlot.itemSlot);
		InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
		inventoryItem.name = itemInstance.sharedData.DisplayName;
		inventoryItem.InitializeItem(itemInstance);
		inventorySlot.SetItemInSlotAfterDrag(inventoryItem);
	}
}
