using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemDroppingController : MonoBehaviour
{
	[SerializeField] ItemDropper itemDropper;

	private void Start() {
		PlayerInventory.Instance.OnItemDropped += itemDropper.DropItem;
	}

	void Update() {
		HandleInventoryItemDropping();
	}

	private void HandleInventoryItemDropping() {
		if (Input.GetKeyDown(KeyCode.Q)) {
			if (InventoryItem.CurrentHoveredItem != null) {
				InventorySlot inventorySlot = InventoryItem.CurrentHoveredItem.GetCurrentInventorySlot();
				Inventory inventory = inventorySlot.GetInventory();
				InventoryItem inventoryItemBeingDropped = InventoryItem.CurrentHoveredItem;
				inventory.DropItem(inventorySlot);
				Destroy(inventoryItemBeingDropped.gameObject);
			}
		}
	}
}
