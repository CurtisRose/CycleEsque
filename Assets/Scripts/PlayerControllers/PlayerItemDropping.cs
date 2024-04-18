using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemDropping : MonoBehaviour
{
    [SerializeField] PlayerInventory playerInventory;
    [SerializeField] PlayerWeaponController playerWeaponController;
    [SerializeField] Transform head;
    [SerializeField] float throwForce;
    [SerializeField] Transform throwPosition;

    void Awake()
    {
        playerWeaponController = GetComponent<PlayerWeaponController>();
        playerInventory.OnItemDropped += DropItem;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInventoryItemDropping();
    }

    private void HandleInventoryItemDropping()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (InventoryItem.CurrentHoveredItem != null)
            {
                //InventoryItem.CurrentHoveredItem
                //InventoryItem.CurrentHoveredItem.item
                InventoryItem inventoryItemBeingDropped = InventoryItem.CurrentHoveredItem;
                inventoryItemBeingDropped.GetCurrentInventorySlot().RemoveItemFromSlot();
                DropItem(InventoryItem.CurrentHoveredItem.itemInstance);
                Destroy(InventoryItem.CurrentHoveredItem.gameObject);
            }
        }
    }

    private void DropItem(ItemInstance itemInstance)
    {
        WorldItem itemBeingDropped = ItemSpawner.Instance.SpawnItem(itemInstance, throwPosition.position, Quaternion.identity);
        //WorldItem itemBeingDropped = Instantiate<WorldItem>(InventoryItem.CurrentHoveredItem.item.itemPrefab, throwPosition.position, Quaternion.identity);
        // Maybe yeet it a little bit
        itemBeingDropped.InitializeFromItemInstance(itemInstance);
        itemBeingDropped.GetComponent<Rigidbody>().isKinematic = false;
        itemBeingDropped.GetComponent<Rigidbody>().useGravity = true;
        itemBeingDropped.GetComponent<Rigidbody>().AddForce(head.forward * throwForce, ForceMode.Impulse);
        // This is so the pick up menu doesn't trigger immediately.
        itemBeingDropped.SetUninteractableTemporarily();
        itemBeingDropped.SetNumberOfStartingItems((int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack));
    }
}
