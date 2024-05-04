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

	struct ConsumableInfo {
		public Rarity rarity;
		public Sprite sprite;
		public ConsumableInfo(Rarity rarity, Sprite sprite) {
			this.rarity = rarity;
			this.sprite = sprite;
		}
	}

	public override void Open() {
		base.Open();
		List<int> slots = PlayerInventory.Instance.GetSlotsByType(ItemType.CONSUMABLE);
		// Loop through each slot, check ID, if it's a new ID, copy that slot and add it as a child to the gridLayout
		List<ConsumableInfo> consumableInfo = new List<ConsumableInfo>();
		List<string> IDs = new List<string>();
		foreach (int slot in slots) {
			SharedItemData itemData = PlayerInventory.Instance.inventorySlots[slot].GetItemInSlot().itemInstance.sharedData;
			if (IDs.Contains(itemData.ID)) {
				continue;
			}
			IDs.Add(itemData.ID);
			ConsumableInfo newInfo = new ConsumableInfo(itemData.Rarity, itemData.SmallImage);
			if (!consumableInfo.Contains(newInfo)) {
				consumableInfo.Add(newInfo);
			}
		}

		// Sort the consumableInfo by rarity
		consumableInfo.Sort((x, y) => x.rarity.CompareTo(y.rarity));
		
		// Create slots for each consumable
		foreach (ConsumableInfo info in consumableInfo) {
			ConsumableSelectionSlot newSlot = Instantiate(slotPrefab, container);
			newSlot.itemImage.sprite = info.sprite;
			newSlot.SetImageColor(info.rarity);
		}
	}

	public override void Close() {
		base.Close();
		// Cleanup all the slots
		foreach (Transform child in container) {
			Destroy(child.gameObject);
		}
	}
}
