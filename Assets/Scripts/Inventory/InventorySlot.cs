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
    InventoryItem itemInSlot;

    Inventory inventory;

    [SerializeField] protected TMP_Text weightText;

    void Awake()
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

    public virtual void OnAddItem(Item item)
    {
        inventory.UpdateWeight(item.Weight);
        weightText.text = item.Weight.ToString();
    }

    public virtual void OnRemoveItem(Item item)
    {
        inventory.UpdateWeight(-item.Weight);
        weightText.text = "";
    }
}
