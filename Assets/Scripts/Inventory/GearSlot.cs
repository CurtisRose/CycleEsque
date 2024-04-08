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


    void Awake()
    {
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


    // Item Added to GearSlot
    public override void OnAddItem(Item item)
    {
        weightText.text = item.Weight.ToString();
    }

    // Item Removed from GearSlot
    public override void OnRemoveItem(Item item)
    {
        weightText.text = "";
    }
}
