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

    [SerializeField] protected bool hasItem = false;

    [SerializeField] protected TMP_Text weightText;
    [SerializeField] protected TMP_Text stackSizeText;

    [SerializeField] List<Color> rarityColors;

    [SerializeField] Image itemBackgroundImage;
    [SerializeField] Image itemBorderImage;

    [SerializeField] bool partOfInventoryWeight;

    [SerializeField] public bool UseLargeImage = false;
    [SerializeField] public bool slotContributesToWeight = true;

    public virtual void Awake()
    {
        inventory = GetComponentInParent<Inventory>();
        weightText.text = "";
        stackSizeText.text = "";
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
        InventorySlot otherSlot = itemComingIn.GetCurrentInventorySlot();

        // If itemslot has item, swap
        if (HasItem())
        {
            InventoryItem itemAlreadyHere = itemInSlot;

            // Check to see if it's too heavy for inventory
            if (this.slotContributesToWeight)
            {
                float weightLimitAfterSwap = inventory.GetInventoryWeightLimit();
                
                // If the other slot is the backpack slot then recalculate the inventory size
                if (itemAlreadyHere.item.ItemType == ItemType.BACKPACK)
                {
                    weightLimitAfterSwap += 
                        ((BackpackItem)itemAlreadyHere.item).CarryCapacity -
                        ((BackpackItem)itemComingIn.item).CarryCapacity;
                }

                if (inventory.currentWeight + itemComingIn.GetTotalWeight() - itemAlreadyHere.GetTotalWeight() > weightLimitAfterSwap)
                {
                    // Put the item back in it's original slot
                    //otherSlot.SetItemInSlotAfterDrag(itemComingIn);
                    return;
                }
            }
            if (otherSlot.slotContributesToWeight)
            {

                float weightLimitAfterSwap = inventory.GetInventoryWeightLimit();
                // If the this slot is the backpack slot then recalculate the inventory size
                if (itemComingIn.item.ItemType == ItemType.BACKPACK)
                {
                    weightLimitAfterSwap +=
                        ((BackpackItem)itemComingIn.item).CarryCapacity -
                        ((BackpackItem)itemAlreadyHere.item).CarryCapacity;
                }

                if (inventory.currentWeight + itemAlreadyHere.GetTotalWeight() - itemComingIn.GetTotalWeight() > weightLimitAfterSwap)
                {
                    // Put the item back in it's original slot
                    //otherSlot.SetItemInSlotAfterDrag(itemComingIn);
                    return;
                }
            }

            Swap(itemComingIn);
        } else
        {
            // Check to see if it's too heavy for inventory
            if (this.slotContributesToWeight)
            {
                float weightLimitAfterSwap = inventory.GetInventoryWeightLimit();
                // If the this slot is the backpack slot then recalculate the inventory size
                if (itemComingIn.item.ItemType == ItemType.BACKPACK)
                {
                    weightLimitAfterSwap -=
                        ((BackpackItem)itemComingIn.item).CarryCapacity;
                }

                if (inventory.currentWeight + itemComingIn.GetTotalWeight() > weightLimitAfterSwap)
                {
                    // Put the item back in it's original slot
                    //otherSlot.SetItemInSlotAfterDrag(itemComingIn);
                    return;
                }
            }
        }

        // Set the parent to this itemSlot
        itemComingIn.GetCurrentInventorySlot().RemoveItemFromSlot();
        itemInSlot = itemComingIn;
        itemComingIn.SetParentAfterDrag(itemSlot);
    }

    public virtual void Swap(InventoryItem incomingItem)
    {
        if (HasItem())
        {
            InventoryItem inventoryItemAlreadyHere = itemInSlot;
            InventorySlot otherSlot = incomingItem.GetCurrentInventorySlot();
            otherSlot.RemoveItemFromSlot();
            RemoveItemFromSlot();

            otherSlot.SetItemInSlotAfterDrag(inventoryItemAlreadyHere);
            SetItemInSlotAfterDrag(incomingItem);

            inventoryItemAlreadyHere.DoThingsAfterMove();
            incomingItem.DoThingsAfterMove();
        } else
        {
            InventorySlot otherSlot = incomingItem.GetCurrentInventorySlot();
            otherSlot.RemoveItemFromSlot();
            SetItemInSlotAfterDrag(incomingItem);
            incomingItem.DoThingsAfterMove();
        }
    }

    // This gets called from InventoryItem when the player finishes the drag of an inventoryItem into a slot (or the orginal slot)
    public virtual void SetItemInSlotAfterDrag(InventoryItem inventoryItem)
    {
        InventorySlot test = this;  
        if (HasItem())
        {
            if (itemInSlot == inventoryItem)
            {
                return;
            }
            Swap(inventoryItem);
        }
        
        {
            itemInSlot = inventoryItem;
            hasItem = true;

            if (slotContributesToWeight)
            {
                inventory.UpdateWeight(inventoryItem.GetTotalWeight());
            }
            weightText.text = inventoryItem.GetTotalWeight().ToString();
            if (inventoryItem.item.stackable)
            {
                stackSizeText.text = inventoryItem.GetItemCount().ToString();
            }
            else
            {
                stackSizeText.text = "";
            }
            inventoryItem.SetParentAfterDrag(itemSlot);
            SetImageColor(inventoryItem.item.Rarity);
        }
        if (itemInSlot != null)
        {
            itemInSlot.OnItemCountChanged += RefreshItemStats;
        }
    }

    protected void RefreshItemStats()
    {
        if(itemInSlot == null)
        {
            weightText.text = "";
            stackSizeText.text = "";

        } else
        {
            weightText.text = (itemInSlot.GetTotalWeight()).ToString();
            stackSizeText.text = itemInSlot.GetItemCount().ToString();
        }
    }

    // This gets called from InventoryItem when the player clicks the inventoryItem and begins to drag it.
    public virtual void RemoveItemFromSlot()
    {
        if (itemInSlot != null)
        {
            InventoryItem itemToReturn = itemInSlot;
            itemInSlot.OnItemCountChanged -= RefreshItemStats;
            if (slotContributesToWeight)
            {
                inventory.UpdateWeight(-(itemInSlot.GetTotalWeight()));
            }
            itemInSlot = null;
            hasItem = false;
            RefreshItemStats();
            SetImageColorDefault();
        }
    }

    public InventoryItem GetItemInSlot()
    {
        return itemInSlot;
    }

    // These are pass through functions from the inventory Item to the slot to the inventory
    // I can't figure out a better way to do it since the item is what knows it's being moved
    // And I don't particularly want the item to know about the inventory
    public virtual void StartInventoryItemMovedPassThrough(InventoryItem inventoryItem)
    {
        inventory.StartInventoryItemMoved(inventoryItem);
    }

    public virtual void EndInventoryItemMovedPassThrough(InventoryItem inventoryItem)
    {
        inventory.EndInventoryItemMoved(inventoryItem);
    }

    public virtual void ItemQuickEquipPassThrough(InventoryItem inventoryItem)
    {
        inventory.QuickEquip(this);
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

    public bool HasItem()
    {
        return hasItem;
    }
}
