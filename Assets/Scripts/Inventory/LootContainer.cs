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

		bool successfullyAdded = playerInventory.AddItem(emptySlot, inventorySlot.GetItemInSlot());

		// I think this will try to effectively quick sort that item
		if (successfullyAdded) {
			playerInventory.QuickEquip(emptySlot);
            return true;
		}
		return false;
	}
}
