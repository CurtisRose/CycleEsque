using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [SerializeField] public Transform itemSlot;
    [SerializeField] protected InventoryItem itemInSlot;

    Inventory inventory;

    [SerializeField] protected bool HasItem = false;

    [SerializeField] protected TMP_Text weightText;

    [SerializeField] List<Color> rarityColors;

    [SerializeField] Image itemBackgroundImage;
    [SerializeField] Image itemBorderImage;

    public virtual void Awake()
    {
        inventory = GetComponentInParent<Inventory>();
        weightText.text = "";
        SetImageColorDefault();
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnDropItem(eventData);
    }

    public virtual void OnDropItem(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        InventoryItem itemComingIn = dropped.GetComponent<InventoryItem>();

        // If itemslot has item, swap
        if (HasItem)
        {
            // Begin Swap
            InventoryItem itemAlreadyHere = itemInSlot;
            itemAlreadyHere.SetParentAfterDrag(itemComingIn.GetCurrentInventorySlot().transform);
            InventorySlot otherSlot = itemComingIn.GetCurrentInventorySlot();
            RemoveItemFromSlot(itemAlreadyHere);
            otherSlot.SetItemInSlot(itemAlreadyHere);
            itemAlreadyHere.DoThingsAfterMove();
            // If the other slot is NOT gear slot and this IS a gear slot, switch the image from the large image to the small.
            if (!(otherSlot as GearSlot) && (this as GearSlot))
            {
                itemAlreadyHere.itemImage.sprite = itemAlreadyHere.item.SmallImage;
            }
        }

        // Set the parent to this itemSlot
        itemInSlot = itemComingIn;
        itemComingIn.SetParentAfterDrag(itemSlot);
    }

    public virtual void Swap(InventoryItem incomingItem)
    {
        if (HasItem)
        {
            InventoryItem inventoryItemAlreadyHere = itemInSlot;
            InventorySlot otherSlot = incomingItem.GetCurrentInventorySlot();
            otherSlot.RemoveItemFromSlot(incomingItem);
            RemoveItemFromSlot(inventoryItemAlreadyHere);

            otherSlot.SetItemInSlot(inventoryItemAlreadyHere);
            SetItemInSlot(incomingItem);

            inventoryItemAlreadyHere.DoThingsAfterMove();
            incomingItem.DoThingsAfterMove();
        } else
        {
            InventorySlot otherSlot = incomingItem.GetCurrentInventorySlot();
            otherSlot.RemoveItemFromSlot(incomingItem);
            SetItemInSlot(incomingItem);
            incomingItem.DoThingsAfterMove();
        }
    }

    // This gets called from InventoryItem when the player finishes the drag of an inventoryItem into a slot (or the orginal slot)
    public virtual void SetItemInSlot(InventoryItem inventoryItem)
    {
        if (HasItem)
        {
            Swap(inventoryItem);
        }
        else
        {
            itemInSlot = inventoryItem;
            HasItem = true;
            inventory.UpdateWeight(inventoryItem.item.Weight);
            weightText.text = inventoryItem.item.Weight.ToString();
            inventoryItem.SetParentAfterDrag(itemSlot);
            SetImageColor(inventoryItem.item.Rarity);
        }
    }

    // This gets called from InventoryItem when the player clicks the inventoryItem and begins to drag it.
    public virtual void RemoveItemFromSlot(InventoryItem inventoryItem)
    {
        itemInSlot = null;
        HasItem = false;
        inventory.UpdateWeight(-inventoryItem.item.Weight);
        weightText.text = "";
        SetImageColorDefault();
    }

    public InventoryItem GetItemInSlot()
    {
        return itemInSlot;
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

    protected void SetImageColor(Rarity rarity)
    {
        Color temp = rarityColors[(int)rarity];
        itemBackgroundImage.color = temp;
        itemBorderImage.color = temp;
    }

    protected void SetImageColorDefault()
    {
        Color temp = Color.white;
        temp.a = 0.2f;
        itemBackgroundImage.color = temp;
        itemBorderImage.color = temp;
    }
}
