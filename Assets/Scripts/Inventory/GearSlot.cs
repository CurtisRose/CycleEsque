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


    public override void Awake()
    {
        base.Awake();
        indicationColor = image.color;
        weightText.text = "";
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
        if (this.itemType == itemType)
        {
            image.color = Color.green;
        }
        else
        {
            image.color = Color.red;
        }
    }

    public void ResetItemIndication()
    {
        image.color = indicationColor;
    }

    public void SetSlotHolderImageVisible(bool setVisible)
    {
        slotIndicationText.enabled = setVisible;
    }

    // This gets called from InventoryItem when the player finishes the drag of an inventoryItem into a slot (or the orginal slot)
    public override void SetItemInSlot(InventoryItem inventoryItem)
    {
        itemInSlot = inventoryItem;
        HasItem = true;
    }

    // This gets called from InventoryItem when the player clicks the inventoryItem and begins to drag it.
    public override void RemoveItemFromSlot(InventoryItem inventoryItem)
    {
        itemInSlot = null;
        HasItem = false;
    }
}
