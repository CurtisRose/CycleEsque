using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public ItemInstance itemInstance;

    [SerializeField] protected Image itemImage;

    [SerializeField] private Transform parentAfterDrag;
    [SerializeField] private InventorySlot currentInventorySlot;

    public delegate void ItemCountChanged();
    public event ItemCountChanged OnItemCountChanged;

    public static InventoryItem CurrentHoveredItem { get; private set; }

    // Initialized by the inventory when it's created
    public void InitializeItem(SharedItemData sharedItemData)
    {
        this.itemInstance.sharedData = sharedItemData;
        itemImage.sprite = sharedItemData.SmallImage;
        parentAfterDrag = transform.parent;
    }

    public void InitializeItem(ItemInstance itemInstance)
    {
        this.itemInstance = itemInstance;
        itemImage.sprite = itemInstance.sharedData.SmallImage;
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
        itemImage.sprite = itemInstance.sharedData.SmallImage;
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

        // Check if the drag ended outside the inventory UI, i.e., no UI element was under the pointer
        GameObject droppedOn = eventData.pointerCurrentRaycast.gameObject;
        if (droppedOn != null && IsPartOfInventoryUI(droppedOn))
        {
            currentInventorySlot.SetItemInSlotAfterDrag(this);
            currentInventorySlot.EndInventoryItemMovedPassThrough(this);
            DoThingsAfterMove();
        }
        else
        {
            //currentInventorySlot.RemoveItemFromSlot();
            currentInventorySlot.DropItem();
            Destroy(this.gameObject);
        }
        itemImage.raycastTarget = true;
    }

    private bool IsPartOfInventoryUI(GameObject obj)
    {
        while (obj != null)
        {
            if (obj.CompareTag("InventoryUI"))
                return true;
            obj = obj.transform.parent?.gameObject;
        }
        return false;
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
        return itemInstance.sharedData.ItemType;
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
            itemImage.sprite = itemInstance.sharedData.LargeImage;
        } else
        {
            itemImage.sprite = itemInstance.sharedData.SmallImage;
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
        return (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
    }

    public void IncrementItemCount()
    {
        ChangeItemCount(1);
    }

    public void ChangeItemCount(int change)
    {
        int currentCount = (int)itemInstance.GetProperty(ItemAttributeKey.NumItemsInStack);
        itemInstance.SetProperty(ItemAttributeKey.NumItemsInStack, currentCount + change);
        if (OnItemCountChanged != null)
        {
            OnItemCountChanged();
        }
    }

    public float GetTotalWeight()
    {
        // I've put several protections in place to make sure non stackable items only have one item in them
        // But this is yet another that their weight won't be fucked up.
        if (itemInstance.sharedData.stackable)
        {
            return GetItemCount() * itemInstance.sharedData.Weight;
        }
        else
        {
            return itemInstance.sharedData.Weight;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CurrentHoveredItem = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CurrentHoveredItem == this)
        {
            CurrentHoveredItem = null;
        }
    }

    private void OnDisable()
    {
        CurrentHoveredItem = null;
    }
}
