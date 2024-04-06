using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image image;
    public Color selectedColor, notSelectedColor;

    void Awake()
    {
        Deselect();
    }

    [SerializeField] public Transform itemSlot;
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        InventoryItem draggableItem = dropped.GetComponent<InventoryItem>();

        // If itemslot has item, swap
        if (itemSlot.childCount != 0)
        {
            // Begin Swap
            Transform itemToSwap = itemSlot.GetChild(0);
            itemToSwap.SetParent(draggableItem.parentAfterDrag);
        }

        draggableItem.parentAfterDrag = itemSlot;
    }

    public void Select()
    {
        image.color = selectedColor;
    }
    public void Deselect()
    {
        image.color = notSelectedColor;
    }
}
