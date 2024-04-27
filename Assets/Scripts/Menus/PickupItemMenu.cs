using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PickupItemMenu : Menu
{
    public static PickupItemMenu Instance;

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
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void UpdatePickupPromptInfo(WorldItem worldItem)
    {
        SharedItemData sharedItemData = worldItem.GetSharedItemData();
        itemName.text = sharedItemData.DisplayName;
        itemWeight.text = worldItem.GetWeight().ToString();
        itemRarity.text = sharedItemData.Rarity.ToString();
        itemRarity.color = RarityColorManager.Instance.GetColorByRarity(sharedItemData.Rarity);
        float roomInBackpack = PlayerInventory.Instance.GetInventoryWeightLimit() - PlayerInventory.Instance.GetCurrentInventoryWeight();
        if (roomInBackpack >= worldItem.GetWeight())
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

        UpdatePickupPromptPosition(worldItem.transform.position);
    }

    void UpdatePickupPromptPosition(Vector3 itemPosition)
    {
        Vector3 uiPosition = itemPosition + Vector3.up * menuHeightAboveItemMultiplier;  // Adjust this offset as needed

        Camera camera = Camera.main;
        Vector3 screenPosition = camera.WorldToScreenPoint(uiPosition);

        // Check if the position is behind the camera
        if (screenPosition.z < 0)
        {
            //menuPanel.SetActive(false);
            //return;
        }

        // Clamp the screen position to stay within the visible screen
        RectTransform menuRect = GetComponent<RectTransform>();
        float width = menuRect.sizeDelta.x * menuRect.lossyScale.x / 2;  // Half width of the menu
        float height = menuRect.sizeDelta.y * menuRect.lossyScale.y / 2; // Half height of the menu

        screenPosition.x = Mathf.Clamp(screenPosition.x, width, Screen.width - width);
        screenPosition.y = Mathf.Clamp(screenPosition.y, height, Screen.height - height);

        menuRect.position = screenPosition;
        menuPanel.gameObject.SetActive(true);
    }
}
