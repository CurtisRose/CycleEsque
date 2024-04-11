using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [HideInInspector] public BaseItem item;
    [SerializeField] protected int count = 1;

    [SerializeField] protected Image itemImage;

    [SerializeField] private Transform parentAfterDrag;
    [SerializeField] private InventorySlot currentInventorySlot;

    public delegate void ItemCountChanged();
    public event ItemCountChanged OnItemCountChanged;

    // Initialized by the inventory when it's created
    public void InitializeItem(BaseItem item)
    {
        this.item = item;
        itemImage.sprite = item.SmallImage;
        parentAfterDrag = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

		//currentInventorySlot.RemoveItemFromSlot();
        currentInventorySlot.StartInventoryItemMovedPassThrough(this);

        parentAfterDrag = transform.parent;
        // Sets the UI Panel at the top of this hierarchy as the parent
        transform.SetParent(transform.root);
        // Then set it at the bottom of that hierarchy so that it is drawn on top of everything else
        transform.SetAsLastSibling();
        itemImage.raycastTarget = false;

        // Changing the sprite is only useful for primary weapons.
        // If the primary weapon is put into a big weapon slot, the image get's changed to the big image
        // It needs to be switched back to the small image when it's dragged
        itemImage.sprite = item.SmallImage;
        AdjustImageSizeForDragging();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        currentInventorySlot.SetItemInSlotAfterDrag(this);
        currentInventorySlot.EndInventoryItemMovedPassThrough(this);

        DoThingsAfterMove();
        itemImage.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Check if the right mouse button was clicked
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Attempt to quick sort it into or out of the gear slots.
            currentInventorySlot.ItemQuickEquipPassThrough(this);
        }
    }

    public void DoThingsAfterMove()
    {
        // Before setting the parent, adjust the size to fit the new slot
        AdjustImageSizeToFitSlot(parentAfterDrag);

        transform.SetParent(parentAfterDrag);
    }

    public ItemType GetItemType()
    {
        return item.ItemType;
    }

    private void AdjustImageSizeForDragging()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 80);
    }

    private void AdjustImageSizeToFitSlot(Transform newParent)
    {
        // Example: Adjust the image size based on the new parent slot's dimensions
        // This could involve setting RectTransform size, applying scaling, or using layout components
        RectTransform parentRectTransform = newParent.GetComponent<RectTransform>();
        RectTransform itemRectTransform = GetComponent<RectTransform>();

        if (currentInventorySlot.UseLargeImage)
        {
            itemImage.sprite = item.LargeImage;
        } else
        {
            itemImage.sprite = item.SmallImage;
        }

        // Set the item as a child of the new parent
        itemRectTransform.SetParent(newParent, false);

        // Optionally, set pivot and anchors to the center if they are not already
        itemRectTransform.pivot = new Vector2(0.5f, 0.5f);
        itemRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        itemRectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        // Adjust the sizeDelta to match the parent's size, or use a specific size here
        itemRectTransform.sizeDelta = new Vector2(parentRectTransform.rect.width, parentRectTransform.rect.height);

        itemRectTransform.localPosition = Vector3.zero;
    }

    public void SetParentAfterDrag(Transform parentAfterDrag)
    {
        this.parentAfterDrag = parentAfterDrag;
        currentInventorySlot = parentAfterDrag.GetComponentInParent<InventorySlot>(true);
    }

    public InventorySlot GetCurrentInventorySlot()
    {
        return currentInventorySlot;
    }

    public int GetItemCount()
    {
        return count;
    }

    public void IncrementItemCount()
    {
        ChangeItemCount(1);
    }

    public void ChangeItemCount(int change)
    {
        count += change;
        if (OnItemCountChanged != null)
        {
            OnItemCountChanged();
        }
    }

    public float GetTotalWeight()
    {
        return GetItemCount() * item.Weight;
    }
}
