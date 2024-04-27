using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LootContainer : Inventory, IInteractable, IActivatable
{
    [SerializeField] Transform throwPosition;
    [SerializeField] float throwForce;
    [SerializeField] string containerName;
    [SerializeField] TMP_Text containerNameText;

    public LootPool itemPool;
    public int numberOfItems;

    [SerializeField] LootBoxMenu lootMenu;

	bool isActive;

	void Awake() {
        containerNameText.text = containerName;
    }

    protected void Start()
    {
		// This lets the inventory slots intitialize
        MenuManager.Instance.OpenMenu(lootMenu);
        MenuManager.Instance.CloseMenu(lootMenu);
    }

	public void Activate() {
		if (isActive) {
			return;
		}
		isActive = true;
		SpawnItems();
	}

	public void Deactivate() {
		if (!isActive) {
			return;
		}
		isActive = false;
		foreach(InventorySlot slot in inventorySlots) {
			if (slot.HasItem()) {
				InventoryItem itemInSlot = RemoveItemFromSlot(slot);
				Destroy(itemInSlot.gameObject);
			}
		}
	}

	public bool IsActive() {
		return isActive;
	}

	private void SpawnItems()
    {
        for(int i = 0; i < numberOfItems; i++)
        {
            // Just quadruple check that there is no weight in here....
            WorldItem selectedItem = itemPool.GetRandomItemWithQuantity();
            if (selectedItem != null)
            {
                ItemInstance itemInstance = selectedItem.CreateItemInstance();
                bool addedItem = AddItem(itemInstance);
            }
        }
    }

    public override void DropItem(InventorySlot inventorySlot)
    {
        ItemInstance itemInstance = inventorySlot.GetItemInSlot().itemInstance;
        WorldItem itemBeingDropped = ItemSpawner.Instance.SpawnItem(itemInstance, throwPosition.position, Quaternion.identity);
        //WorldItem itemBeingDropped = Instantiate<WorldItem>(InventoryItem.CurrentHoveredItem.item.itemPrefab, throwPosition.position, Quaternion.identity);
        // Maybe yeet it a little bit
        itemBeingDropped.InitializeFromItemInstance(itemInstance);
        itemBeingDropped.GetComponent<Rigidbody>().isKinematic = false;
        itemBeingDropped.GetComponent<Rigidbody>().useGravity = true;
        itemBeingDropped.GetComponent<Rigidbody>().AddForce(throwPosition.forward * throwForce, ForceMode.Impulse);
        // This is so the pick up menu doesn't trigger immediately.
        itemBeingDropped.SetUninteractableTemporarily();
        itemBeingDropped.SetNumberOfStartingItems((int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack));
    }

    public void Interact()
    {
        MenuManager.Instance.CloseMenu(LootBoxInteractMenu.Instance);
        MenuManager.Instance.OpenMenu(lootMenu);
        //LootBoxInteractMenu.Instance.Close();
        //lootMenu.Open();
    }

    public void ShowUI()
    {
        if (!lootMenu.IsOpen())
        {
            MenuManager.Instance.OpenMenu(LootBoxInteractMenu.Instance);
            LootBoxInteractMenu.Instance.UpdatePickupPromptPosition(transform.position);
        }
    }

    public void HideUI()
    {
        MenuManager.Instance.CloseMenu(LootBoxInteractMenu.Instance);
        MenuManager.Instance.CloseMenu(lootMenu);
        //LootBoxInteractMenu.Instance.Close();
        //lootMenu.Close();
    }

    public bool IsInteractable()
    {
        return true;
    }

	public override bool QuickEquip(InventorySlot inventorySlot) {
		PlayerInventory playerInventory = PlayerInventory.Instance;
		InventorySlot emptySlot = playerInventory.FindEarliestEmptySlot();
		bool success = false;

		// Try to 
		if (emptySlot != null) {
			if (playerInventory.CanAddItem(emptySlot, inventorySlot.GetItemInSlot())) {
				if (inventorySlot.GetItemInSlot().itemInstance.sharedData.Stackable) {
					int numItems = inventorySlot.GetItemInSlot().GetItemCount();
					// loop through each slot in the inventory and try to combine them
					foreach (InventorySlot slot in playerInventory.inventorySlots) {
						if (slot.HasItem() && slot.GetItemInSlot().itemInstance.sharedData.ID == inventorySlot.GetItemInSlot().itemInstance.sharedData.ID) {
							numItems = playerInventory.Combine(slot, inventorySlot.GetItemInSlot());
							if (numItems == 0) {
								return true;
							}
						}
					}
				}

				success = playerInventory.Swap(emptySlot, inventorySlot.GetItemInSlot());
				if (success) {
					return true;
				}
			} else {
				// Can't add whole stack
				// Try to add as many as possible
				if (inventorySlot.GetItemInSlot().itemInstance.sharedData.Stackable) {
					int numItems = inventorySlot.GetItemInSlot().GetItemCount();
					float weightLeft = playerInventory.GetInventoryWeightLimit() - playerInventory.GetCurrentInventoryWeight();
					int numItemsToHold = (int)Mathf.Floor(weightLeft / inventorySlot.GetItemInSlot().itemInstance.sharedData.Weight);
					if (numItemsToHold == 0) {
						return false; // Can't add any
					}
					inventorySlot.GetItemInSlot().AddToItemCount(-numItemsToHold);

					// loop through each slot in the inventory and try to combine them
					foreach (InventorySlot slot in playerInventory.inventorySlots) {
						if (slot.HasItem() && slot.GetItemInSlot().itemInstance.sharedData.ID == inventorySlot.GetItemInSlot().itemInstance.sharedData.ID) {
							numItemsToHold = playerInventory.Combine(slot, inventorySlot.GetItemInSlot());
							if (numItemsToHold == 0) {
								return true;
							}
						}
					}

					// There are items leftover to add, create new instance and add them
					ItemInstance newItemInstance = new ItemInstance(inventorySlot.GetItemInSlot().itemInstance.sharedData);
					newItemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, numItemsToHold);
					playerInventory.AddItem(newItemInstance);
					return false; // Not all were added
				} else {
					// Can't add because not stackable
					return false;
				}
			}
		}

		return base.QuickEquip(inventorySlot);
	}
}
