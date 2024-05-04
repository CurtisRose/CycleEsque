using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItemTracker : MonoBehaviour
{
	[SerializeField] private SharedItemData itemDataToTrack;

    int numberOfItems = 0;

	public delegate void NumberOfItemsChanged(int numberOfItems);
	public event NumberOfItemsChanged OnNumberOfItemsChanged;

	private void Start() {
		PlayerInventory.Instance.OnInventoryChanged += InventoryChanged;
	}

	public void InventoryChanged() {
		int newNumberOfItems = PlayerInventory.Instance.GetNumberOfItems(itemDataToTrack.ID);
		if (numberOfItems != newNumberOfItems) {
			numberOfItems = newNumberOfItems;
			OnNumberOfItemsChanged?.Invoke(numberOfItems);
		}
	}

	public void SetNewItemToTrack(SharedItemData itemData) {
		itemDataToTrack = itemData;
		InventoryChanged();
	}
}
