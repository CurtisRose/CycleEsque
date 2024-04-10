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
        base.SetItemInSlotAfterDrag(inventoryItem);
        SetSlotHolderImageVisible(false);
        SetImageColor(inventoryItem.item.Rarity);

        if (OnGearSlotsChanged != null)
        {
            OnGearSlotsChanged(this);
        }
    }

    // This gets called from InventoryItem when the player clicks the inventoryItem and begins to drag it.
    public override void RemoveItemFromSlot()
    {
        //Debug.Log("Item removed from gear slot");
        if (itemInSlot != null)
        {
            SetSlotHolderImageVisible(true);
            SetImageColorDefault();
        }
        base.RemoveItemFromSlot();
        if (OnGearSlotsChanged != null)
        {
            OnGearSlotsChanged(this);
        }
    }

    public ItemType GetItemType()
    {
        return itemType;
    }
}
