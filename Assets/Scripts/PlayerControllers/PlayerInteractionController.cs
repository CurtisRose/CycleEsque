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

    WorldItem itemLookingAt;

    [SerializeField] Image proximityInteractionIndicator;

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
                if (Input.GetKeyDown(KeyCode.F))
                {
                    OnWorldItemPickedUp(worldItem);
                } else
                {
                    itemLookingAt = worldItem;
                    itemLookingAt.ShowUI();
                }
            }
        }
        else
        {
            if (itemLookingAt != null)
            {
                itemLookingAt.HideUI();
                itemLookingAt = null;
            }
        }
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            proximityInteractionIndicator.enabled = true;
        } else
        {
            proximityInteractionIndicator.enabled = false;
        }
    }

    void OnWorldItemPickedUp(WorldItem item)
    {
        // Hide prompt after picking up the item
        bool pickedUp = playerInventory.AddItem(item);
        if (pickedUp)
        {
            item.PickupItem();
            item.HideUI();
            Destroy(item.gameObject);
        }
    }
}
