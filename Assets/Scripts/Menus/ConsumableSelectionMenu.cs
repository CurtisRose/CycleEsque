using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableSelectionMenu : Menu, IPlayerInitializable
{
    public static ConsumableSelectionMenu Instance;
	ConsumableController consumableController;
	[SerializeField] Transform container;
	[SerializeField] ConsumableSelectionSlot slotPrefab;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy(gameObject);
		}
	}

	public void Initialize() {
		consumableController = Player.Instance.GetComponent<ConsumableController>();
	}

	public override void Open() {
		base.Open();
		List<int> slots = PlayerInventory.Instance.GetSlotsByType(ItemType.CONSUMABLE);
		// Loop through each slot, check ID, if it's a new ID, copy that slot and add it as a child to the gridLayout
		List<SharedItemData> consumableInfo = new List<SharedItemData>();
		List<string> IDs = new List<string>();
		foreach (int slot in slots) {
			SharedItemData itemData = PlayerInventory.Instance.inventorySlots[slot].GetItemInSlot().itemInstance.sharedData;
			if (IDs.Contains(itemData.ID)) {
				continue;
			}
			IDs.Add(itemData.ID);
			if (!consumableInfo.Contains(itemData)) {
				consumableInfo.Add(itemData);
			}
		}

		// Sort the consumableInfo by rarity
		consumableInfo.Sort((x, y) => x.Rarity.CompareTo(y.Rarity));
		
		// Create slots for each consumable
		foreach (SharedItemData info in consumableInfo) {
			ConsumableSelectionSlot newSlot = Instantiate(slotPrefab, container);
			newSlot.Initialize(info);
		}
	}

	public override void Close() {
		base.Close();
		if (ConsumableSelectionSlot.CurrentHoveredConsumable != null) {
			SharedItemData itemData = ConsumableSelectionSlot.CurrentHoveredConsumable.itemData;
			consumableController.SetConsumableToUse((HealthItem)itemData);
			ConsumableVisualizer.Instance.SetInjectorData(itemData);
		}
		// Cleanup all the slots
		foreach (Transform child in container) {
			Destroy(child.gameObject);
		}
	}
}
