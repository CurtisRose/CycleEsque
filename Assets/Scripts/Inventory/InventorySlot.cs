using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image image;
    [SerializeField] public Transform itemSlot;
    protected InventoryItem itemInSlot;

    Inventory inventory;

    [SerializeField] protected bool HasItem = false;

    [SerializeField] protected TMP_Text weightText;

    public virtual void Awake()
    {
        inventory = GetComponentInParent<Inventory>();
        weightText.text = "";
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnDropItem(eventData);
    }

    public virtual void OnDropItem(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        itemInSlot = dropped.GetComponent<InventoryItem>();

        // If itemslot has item, swap
        if (itemSlot.childCount != 0)
        {
            // Begin Swap
            Transform itemToSwap = itemSlot.GetChild(0);
            itemToSwap.SetParent(itemInSlot.parentAfterDrag);
        }

        // Set the parent to this itemSlot
        itemInSlot.parentAfterDrag = itemSlot;
    }

    // This gets called from InventoryItem when the player finishes the drag of an inventoryItem into a slot (or the orginal slot)
    public virtual void SetItemInSlot(InventoryItem inventoryItem)
    {
        itemInSlot = inventoryItem;
        HasItem = true;
        inventory.UpdateWeight(inventoryItem.item.Weight);
    }

    // This gets called from InventoryItem when the player clicks the inventoryItem and begins to drag it.
    public virtual void RemoveItemFromSlot(InventoryItem inventoryItem)
    {
        itemInSlot = null;
        HasItem = false;
        inventory.UpdateWeight(-inventoryItem.item.Weight);
    }

    // These are pass through functions from the inventory Item to the slot to the inventory
    // I can't figure out a better way to do it since the item is what knows it's being moved
    // And I don't particularly want the item to know about the inventory
    public virtual void StartInventoryItemMoved(InventoryItem inventoryItem)
    {
        inventory.StartInventoryItemMoved(inventoryItem);
    }

    public virtual void EndInventoryItemMoved(InventoryItem inventoryItem)
    {
        inventory.EndInventoryItemMoved(inventoryItem);
    }
}
