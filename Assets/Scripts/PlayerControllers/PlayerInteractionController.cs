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
            if (worldItem != null)
            {
                if (worldItem.IsInteractable())
                {
                    // Show the pickup prompt UI element if a WorldItem is hit.
                    itemLookingAt = worldItem;
                    ShowPickupPrompt(true);

                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        OnWorldItemPickedUp(worldItem);
                    }
                }
            }
        }
        else
        {
            // Hide the pickup prompt UI element if no WorldItem is hit.
            ShowPickupPrompt(false);
        }
    }

    void OnWorldItemPickedUp(WorldItem item)
    {
        // Hide prompt after picking up the item
        bool pickedUp = playerInventory.AddItem(item, item.GetNumberOfItems());
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
        BaseItem item = itemLookingAt.GetBaseItem();
        itemName.text = item.DisplayName;
        itemWeight.text = (itemLookingAt.GetWeight()).ToString();
        itemRarity.text = item.Rarity.ToString();
        itemRarity.color = RarityColorManager.Instance.GetColorByRarity(item.Rarity);
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
}
