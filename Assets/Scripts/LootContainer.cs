using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LootContainer : Inventory, IInteractable
{
    [SerializeField] Transform throwPosition;
    [SerializeField] float throwForce;
    [SerializeField] string containerName;
    [SerializeField] TMP_Text containerNameText;

    public LootPool itemPool;
    public int numberOfItems;

    [SerializeField] LootBoxMenu lootMenu;

    protected void Start()
    {
        MenuManager.Instance.OpenMenu(lootMenu);
        //containerNameText.text = containerName;
        SpawnItem();
        MenuManager.Instance.CloseMenu(lootMenu);
    }

    private void SpawnItem()
    {
        for(int i = 0; i < numberOfItems; i++)
        {
            // Just quadruple check that there is no weight in here....
            currentWeight = 0;

            var (selectedItem, quantity) = itemPool.GetRandomItemWithQuantity();
            if (selectedItem != null)
            {
                ItemInstance itemInstance = selectedItem.CreateItemInstance();
                if (itemInstance.sharedData.Stackable)
                {
                    itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, quantity);
                } else
                {
                    itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, 1);
                }
                bool addedItem = AddItem(itemInstance);
            }
        }
    }

    public override float GetInventoryWeightLimit()
    {
        return 10000;
    }


    public override void QuickEquip(InventorySlot inventorySlot)
    {
        AddItemToEarliestEmptySlot(inventorySlot.GetItemInSlot());
    }

    public override void DropItem(ItemInstance itemInstance)
    {
        WorldItem itemBeingDropped = PlayerItemSpawner.Instance.SpawnItem(itemInstance, throwPosition.position, Quaternion.identity);
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
}
