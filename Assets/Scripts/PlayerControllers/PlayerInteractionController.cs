using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] PlayerInventory playerInventory;
    [SerializeField] float interactionDistance = 5f;
    [SerializeField] LayerMask interactionLayer;
    [SerializeField] Menu pickupPromptMenu;

    WorldItem itemLookingAt;

    // UI Elements from Pickup Window
    [SerializeField] TMP_Text itemName;
    [SerializeField] TMP_Text itemWeight;
    [SerializeField] TMP_Text itemRarity;
    [SerializeField] TMP_Text itemDescription;
    [SerializeField] TMP_Text pickupText;
    [SerializeField] Image pickupImage;

    [SerializeField] float menuHeightAboveItemMultiplier;

    private void Awake()
    {
        pickupPromptMenu.Close();
    }

    void Update()
    {
        DetectAndInteractWithWorldItem();
    }

    void DetectAndInteractWithWorldItem()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
        {
            WorldItem worldItem = hit.collider.GetComponentInParent<WorldItem>();
            if (worldItem != null && worldItem.IsInteractable())
            {
                itemLookingAt = worldItem;
                ShowPickupPrompt(true);
                UpdatePickupPromptPosition();

                if (Input.GetKeyDown(KeyCode.F))
                {
                    OnWorldItemPickedUp(worldItem);
                }
            }
        }
        else
        {
            ShowPickupPrompt(false);
        }
    }

    void OnWorldItemPickedUp(WorldItem item)
    {
        // Hide prompt after picking up the item
        bool pickedUp = playerInventory.AddItem(item);
        if (pickedUp)
        {
            Destroy(item.gameObject);
        }
        ShowPickupPrompt(true);
    }

    void ShowPickupPrompt(bool show)
    {
        if (show)
        {
            UpdatePickupPromptInfo();
        }
        // Only fill out the details once
        if (!pickupPromptMenu.IsOpen() && show)
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.OpenMenu(pickupPromptMenu);
            }
        }
        else if (!show)
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.CloseMenu(pickupPromptMenu);
            }
        }
    }

    void UpdatePickupPromptInfo()
    {
        SharedItemData sharedItemData = itemLookingAt.GetBaseItem();
        itemName.text = sharedItemData.DisplayName;
        itemWeight.text = (itemLookingAt.GetWeight()).ToString();
        itemRarity.text = sharedItemData.Rarity.ToString();
        itemRarity.color = RarityColorManager.Instance.GetColorByRarity(sharedItemData.Rarity);
        float roomInBackpack = playerInventory.GetInventoryWeightLimit() - playerInventory.currentWeight;
        if (roomInBackpack >= itemLookingAt.GetWeight())
        {
            itemWeight.color = Color.white;
            pickupImage.color = Color.white;
            pickupText.color = Color.white;
        }
        else
        {
            itemWeight.color = Color.red;
            pickupImage.color = Color.red;
            pickupText.color = Color.red;
        }

        //itemDescription.text = item.Description;
    }

    void UpdatePickupPromptPosition()
    {
        if (itemLookingAt != null)
        {
            Vector3 itemPosition = itemLookingAt.transform.position;
            Vector3 uiPosition = itemPosition + Vector3.up * menuHeightAboveItemMultiplier;  // Adjust this offset as needed

            Camera camera = Camera.main;
            Vector3 screenPosition = camera.WorldToScreenPoint(uiPosition);

            // Check if the position is behind the camera
            if (screenPosition.z < 0)
            {
                pickupPromptMenu.gameObject.SetActive(false);
                return;
            }

            // Clamp the screen position to stay within the visible screen
            RectTransform menuRect = pickupPromptMenu.GetComponent<RectTransform>();
            float width = menuRect.sizeDelta.x * menuRect.lossyScale.x / 2;  // Half width of the menu
            float height = menuRect.sizeDelta.y * menuRect.lossyScale.y / 2; // Half height of the menu

            screenPosition.x = Mathf.Clamp(screenPosition.x, width, Screen.width - width);
            screenPosition.y = Mathf.Clamp(screenPosition.y, height, Screen.height - height);

            menuRect.position = screenPosition;
            pickupPromptMenu.gameObject.SetActive(true);
        }
        else
        {
            pickupPromptMenu.gameObject.SetActive(false);
        }
    }
}
