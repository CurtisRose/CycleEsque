using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [HideInInspector] public Item item;
    [HideInInspector] public int count = 1;

    public TMP_Text countText;
    public Image itemImage;
    
    [HideInInspector] public Transform parentAfterDrag;
    Inventory inventory;

    public void InitializeItem(Item item, Inventory inventory)
    {
        this.item = item;
        this.inventory = inventory;
        itemImage.sprite = item.image;
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
        
        parentAfterDrag = transform.parent;
        // Sets the UI Panel at the top of this hierarchy as the parent
        transform.SetParent(transform.root);
        // Then set it at the bottom of that hierarchy so that it is drawn on top of everything else
        transform.SetAsLastSibling();
        itemImage.raycastTarget = false;
        inventory.OnItemStartDragged(this);

        if (parentAfterDrag.GetComponentInParent<GearSlot>())
        {
            parentAfterDrag.GetComponentInParent<GearSlot>().SetSlotHolderImageVisible(true);
            parentAfterDrag.GetComponentInParent<InventorySlot>().OnRemoveItem(item);
        } else if (parentAfterDrag.GetComponentInParent<InventorySlot>())
        {
            parentAfterDrag.GetComponentInParent<InventorySlot>().OnRemoveItem(item);
        }
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

        // Before setting the parent, adjust the size to fit the new slot
        AdjustImageSizeToFitSlot(parentAfterDrag);

        transform.SetParent(parentAfterDrag);
        itemImage.raycastTarget = true;
        inventory.OnItemStopDragged(this);

        if (parentAfterDrag.GetComponentInParent<GearSlot>())
        {
            parentAfterDrag.GetComponentInParent<GearSlot>().SetSlotHolderImageVisible(false);
        }

        if (parentAfterDrag.GetComponentInParent<InventorySlot>())
        {
            parentAfterDrag.GetComponentInParent<InventorySlot>().OnAddItem(item);
        }

        AdjustImageSizeForDragging();
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
        // Example: Reset to default size or apply specific adjustments for dragging

    }

    private void AdjustImageSizeToFitSlot(Transform newParent)
    {
        // Example: Adjust the image size based on the new parent slot's dimensions
        // This could involve setting RectTransform size, applying scaling, or using layout components
        RectTransform itemRectTransform = GetComponent<RectTransform>();

        // Set the item as a child of the new parent
        itemRectTransform.SetParent(newParent, false);

        // Set anchors to stretch to fill the parent
        itemRectTransform.anchorMin = Vector2.zero; // Anchors to the bottom-left
        itemRectTransform.anchorMax = Vector2.one;  // Anchors to the top-right

        // Set offsets to zero to fill the parent
        itemRectTransform.offsetMin = Vector2.zero; // Offset from the bottom-left
        itemRectTransform.offsetMax = Vector2.zero; // Offset from the top-right

        // Optional: If you're using a Preserve Aspect Ratio component and want to adjust settings
        var preserveAspect = itemRectTransform.GetComponent<AspectRatioFitter>();
        if (preserveAspect != null)
        {
            preserveAspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            // Adjust other settings as necessary
        }
    }
}
