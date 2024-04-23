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

    [SerializeField] Image itemBackgroundImage;
    [SerializeField] Image itemBorderImage;

    [SerializeField] public bool UseLargeImage = false;
    [SerializeField] public bool slotContributesToWeight = true;

    [SerializeField] public bool partOfPlayerInventory;

    public virtual void Awake()
    {
        inventory = GetComponentInParent<Inventory>(true);
        weightText.text = "";
        stackSizeText.text = "";
        SetImageColorDefault();
    }

    // Happens before OnEndDrag in the InventoryItem
    public virtual void OnDrop(PointerEventData eventData)
    {
        // You can't drag an item with anything but left click.
        // But, apparently, if you drag with middle or right click, it won't visually do anything,
        // But it still calls this method.
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        GameObject dropped = eventData.pointerDrag;
        InventoryItem itemComingIn = dropped.GetComponent<InventoryItem>();
        InventorySlot otherSlot = itemComingIn.GetCurrentInventorySlot();

        if (otherSlot == this)
        {
            return;
        }

        // If itemslot has item, swap, unless stackable and same item type
        if (HasItem())
        {
            if(itemComingIn.itemInstance.sharedData.Stackable && 
                itemComingIn.GetItemType() == itemInSlot.GetItemType())
            {
                // TODO: Need to do something like fill it with what it can
                return;
            }

            InventoryItem itemAlreadyHere = itemInSlot;
            float weightAfterSwap = inventory.currentWeight;
            float weightLimitAfterSwap = inventory.GetInventoryWeightLimit();

            // Check to see if it's too heavy for inventory
            if (this.slotContributesToWeight)
            {                
                // If the other slot is the backpack slot then recalculate the inventory size
                if ((this as GearSlot || otherSlot as GearSlot) && itemInSlot.GetItemType() == ItemType.BACKPACK)
                {
                    weightLimitAfterSwap += 
                        ((BackpackItem)itemAlreadyHere.itemInstance.sharedData).CarryCapacity -
                        ((BackpackItem)itemComingIn.itemInstance.sharedData).CarryCapacity;
                }

                weightAfterSwap = weightAfterSwap + itemComingIn.GetTotalWeight() - itemAlreadyHere.GetTotalWeight();
            }
            if (otherSlot.slotContributesToWeight)
            {
                // If the this slot is the backpack slot then recalculate the inventory size
                if ((this as GearSlot || otherSlot as GearSlot) && itemInSlot.GetItemType() == ItemType.BACKPACK)
                {
                    weightLimitAfterSwap +=
                        ((BackpackItem)itemComingIn.itemInstance.sharedData).CarryCapacity -
                        ((BackpackItem)itemAlreadyHere.itemInstance.sharedData).CarryCapacity;
                }

                weightAfterSwap = weightAfterSwap + itemAlreadyHere.GetTotalWeight() - itemComingIn.GetTotalWeight();
            }

            if (weightAfterSwap > weightLimitAfterSwap)
            {
                return;
            }

            inventory.Swap(this, itemComingIn);
        } 
        else
        {
            // Check to see if it's too heavy for inventory
            if (this.slotContributesToWeight)
            {
                if (!itemComingIn.GetCurrentInventorySlot().slotContributesToWeight)
                {
                    float weightLimitAfterSwap = inventory.GetInventoryWeightLimit();
                    // If the this slot is the backpack slot then recalculate the inventory size
                    if (itemComingIn.itemInstance.sharedData.ItemType == ItemType.BACKPACK &&
                        itemComingIn.GetCurrentInventorySlot() as GearSlot)
                    {
                        weightLimitAfterSwap -=
                            ((BackpackItem)itemComingIn.itemInstance.sharedData).CarryCapacity;
                    }

                    if (inventory.currentWeight + itemComingIn.GetTotalWeight() > weightLimitAfterSwap)
                    {
                        // Try Splitting it if its stackable
                        if (itemComingIn.itemInstance.sharedData.Stackable)
                        {
                            inventory.MoveAsManyAsYouCan(this, itemComingIn);
                        }
                        return;
                    }
                }
            }
        }

        // Set the parent to this itemSlot
        itemComingIn.GetCurrentInventorySlot().RemoveItemFromSlot();
        itemInSlot = itemComingIn;
        itemComingIn.SetParentAfterDrag(itemSlot);
    }

    // This gets called from InventoryItem when the player finishes the drag of an inventoryItem into a slot (or the orginal slot)
    public virtual void SetItemInSlotAfterDrag(InventoryItem inventoryItem)
    {
        if (HasItem())
        {
            if (itemInSlot == inventoryItem)
            {
                return;
            }
            inventory.Swap(this, inventoryItem);
        }
        
        {
            itemInSlot = inventoryItem;
            hasItem = true;

            if (slotContributesToWeight)
            {
                inventory.UpdateWeight(inventoryItem.GetTotalWeight());
            }
            weightText.text = inventoryItem.GetTotalWeight().ToString();
            if (inventoryItem.itemInstance.sharedData.Stackable)
            {
                stackSizeText.text = inventoryItem.GetItemCount().ToString();
            }
            else
            {
                stackSizeText.text = "";
            }
            inventoryItem.SetParentAfterDrag(itemSlot);
            SetImageColor(inventoryItem.itemInstance.sharedData.Rarity);
        }

        if (itemInSlot != null)
        {
            itemInSlot.OnItemCountChanged += RefreshItemStats;
        }
    }

    public void RefreshItemStats()
    {
        if(itemInSlot == null)
        {
            weightText.text = "";
            stackSizeText.text = "";

        } else
        {
            weightText.text = (itemInSlot.GetTotalWeight()).ToString();
            if (itemInSlot.itemInstance.sharedData.Stackable)
            {
                stackSizeText.text = itemInSlot.GetItemCount().ToString();
            }
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

    protected void SetImageColor(Rarity rarity)
    {
        Color temp = RarityColorManager.Instance.GetColorByRarity(rarity);
        itemBackgroundImage.color = temp;
        itemBorderImage.color = temp;
    }

    protected void SetImageColorDefault()
    {
        Color temp = Color.white;
        temp.a = 0.2f;
        itemBackgroundImage.color = temp;
        temp = Color.black;
        temp.a = 0.5f;
        itemBorderImage.color = temp;
    }

    public bool HasItem()
    {
        return hasItem;
    }

    public void DropItem()
    {
        ItemInstance itemInstance = itemInSlot.itemInstance;
        int numItems = itemInSlot.GetItemCount();
        RemoveItemFromSlot();
        inventory.DropItem(itemInstance);
    }

    public Inventory GetInventory()
    {
        return inventory;
    }
}
