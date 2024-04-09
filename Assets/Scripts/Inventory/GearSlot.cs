using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GearSlot : InventorySlot
{
    [SerializeField] ItemType itemType;
    Color indicationColor;
    [SerializeField] TMP_Text slotIndicationText;
    [SerializeField] Image indicationImage;

    public delegate void GearSlotsChanged(GearSlot gearSlot);
    public event GearSlotsChanged OnGearSlotsChanged;

    public override void Awake()
    {
        base.Awake();
        weightText.text = "";
        indicationImage.enabled = false;
    }

    public override void OnDropItem(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        InventoryItem draggableItem = dropped.GetComponent<InventoryItem>();

        if (itemType != draggableItem.GetItemType())
        {
            return;
        }

        base.OnDropItem(eventData);
    }

    public void DisplayItemIndication(ItemType itemType)
    {
        indicationImage.enabled = true;
        if (this.itemType == itemType)
        {
            Color temp = Color.green;
            temp.a = 1.0f;
            indicationImage.color = temp;
        }
        else
        {
            Color temp = Color.red;
            temp.a = 1.0f;
            indicationImage.color = temp;
        }
    }

    public void ResetItemIndication()
    {
        indicationImage.enabled = false;
    }

    public void SetSlotHolderImageVisible(bool setVisible)
    {
        slotIndicationText.enabled = setVisible;
    }

    // This gets called from InventoryItem when the player finishes the drag of an inventoryItem into a slot (or the orginal slot)
    public override void SetItemInSlotAfterDrag(InventoryItem inventoryItem)
    {
        //Debug.Log("Item added to gear slot");
        if (OnGearSlotsChanged != null)
        {
            OnGearSlotsChanged(this);
        }
        base.SetItemInSlotAfterDrag(inventoryItem);
        SetSlotHolderImageVisible(false);
        SetImageColor(inventoryItem.item.Rarity);
    }

    // This gets called from InventoryItem when the player clicks the inventoryItem and begins to drag it.
    public override InventoryItem RemoveItemFromSlot()
    {
        //Debug.Log("Item removed from gear slot");
        if (OnGearSlotsChanged != null)
        {
            OnGearSlotsChanged(this);
        }
        if (itemInSlot != null)
        {
            SetSlotHolderImageVisible(true);
            SetImageColorDefault();
        }
        return base.RemoveItemFromSlot();
    }

    public ItemType GetItemType()
    {
        return itemType;
    }
}
