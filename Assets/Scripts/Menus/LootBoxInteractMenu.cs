using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBoxInteractMenu : Menu
{
    public static LootBoxInteractMenu Instance;
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

    public void UpdatePickupPromptPosition(Vector3 itemPosition)
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
