using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemDropping : MonoBehaviour
{
    [SerializeField] PlayerWeaponController playerWeaponController;
    [SerializeField] Transform head;
    [SerializeField] float throwForce;
    [SerializeField] Transform throwPosition;

    void Awake()
    {
        playerWeaponController = GetComponent<PlayerWeaponController>();
    }

	private void Start() {
		PlayerInventory.Instance.OnItemDropped += DropItem;
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
                InventorySlot inventorySlot = InventoryItem.CurrentHoveredItem.GetCurrentInventorySlot();
                Inventory inventory = inventorySlot.GetInventory();
				InventoryItem inventoryItemBeingDropped = InventoryItem.CurrentHoveredItem;
				inventory.DropItem(inventorySlot);
                Destroy(inventoryItemBeingDropped.gameObject);
            }
        }
    }

    private void DropItem(ItemInstance itemInstance)
    {
        WorldItem itemBeingDropped = PlayerItemSpawner.Instance.SpawnItem(itemInstance, throwPosition.position, Quaternion.identity);
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
