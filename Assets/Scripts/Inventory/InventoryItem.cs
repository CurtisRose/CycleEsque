using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [HideInInspector] public BaseItem item;
    [HideInInspector] public int count = 1;

    public TMP_Text countText;
    public Image itemImage;
    
    [HideInInspector] public Transform parentAfterDrag;

    // Initialized by the inventory when it's created
    public void InitializeItem(BaseItem item)
    {
        this.item = item;
        itemImage.sprite = item.Image;
        parentAfterDrag = transform.parent;
        RefreshItemCount();
    }

    public void RefreshItemCount()
    {
        countText.text = count.ToString();
        bool textActive = count > 1;
        countText.gameObject.SetActive(textActive);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        parentAfterDrag.GetComponentInParent<InventorySlot>().RemoveItemFromSlot(this);
        parentAfterDrag.GetComponentInParent<InventorySlot>().StartInventoryItemMoved(this);

        parentAfterDrag = transform.parent;
        // Sets the UI Panel at the top of this hierarchy as the parent
        transform.SetParent(transform.root);
        // Then set it at the bottom of that hierarchy so that it is drawn on top of everything else
        transform.SetAsLastSibling();
        itemImage.raycastTarget = false;

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

        parentAfterDrag.GetComponentInParent<InventorySlot>().SetItemInSlot(this);
        parentAfterDrag.GetComponentInParent<InventorySlot>().EndInventoryItemMoved(this);

        // Before setting the parent, adjust the size to fit the new slot
        AdjustImageSizeToFitSlot(parentAfterDrag);

        transform.SetParent(parentAfterDrag);
        itemImage.raycastTarget = true;
    }

    public ItemType GetItemType()
    {
        return item.ItemType;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Check if the right mouse button was clicked
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Attempt to quick sort it into or out of the gear slots.

        }
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
}
