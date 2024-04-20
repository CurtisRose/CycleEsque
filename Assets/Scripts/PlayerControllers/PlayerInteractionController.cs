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

    IInteractable interactableObjectLookingAt;

    [SerializeField] Image proximityInteractionIndicator;

    void Update()
    {
        DetectAndInteractWithInteractableObject();
    }

    void DetectAndInteractWithInteractableObject()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
        {
            IInteractable interactableObject = hit.collider.GetComponentInParent<IInteractable>();
            if (interactableObject != null && interactableObject.IsInteractable())
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    //OnWorldItemPickedUp(interactableObject);
                    interactableObject.Interact();
                } else
                {
                    interactableObjectLookingAt = interactableObject;
                    interactableObjectLookingAt.ShowUI();
                }   
            }
        }
        else
        {
            if (interactableObjectLookingAt != null)
            {
                interactableObjectLookingAt.HideUI();
                interactableObjectLookingAt = null;
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
}
