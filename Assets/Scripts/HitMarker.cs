using UnityEngine;
using System.Collections;

public class HitMarker : MonoBehaviour
{
    public static HitMarker Instance { get; private set; }
    [SerializeField] GameObject hitMarkerObject;
    [SerializeField] float duration;  // Duration for how long the hit marker should be visible
    [SerializeField] float flashSpeed;
    bool active;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        active = false;
    }

    public void ShowHitMarker( )
    {
        if (active)
        {
            // If already active, flash off for a moment and then resume being on
            StopAllCoroutines();  // Stop any running coroutine to handle a new flash
            StartCoroutine(FlashHitMarker());
        }
        else
        {
            // Else, activate as normal
            active = true;
            hitMarkerObject.SetActive(true);
            StartCoroutine(DeactivateAfterDelay());
        }
    }

    private IEnumerator FlashHitMarker()
    {
        hitMarkerObject.SetActive(false);
        yield return new WaitForSeconds(flashSpeed);  // Wait for the off-flash duration
        hitMarkerObject.SetActive(true);
        StartCoroutine(DeactivateAfterDelay());  // Continue to deactivate after the full duration
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(duration);  // Wait for the duration before hiding
        HideHitMarker();
    }

    private void HideHitMarker()
    {
        active = false;
        hitMarkerObject.SetActive(false);  // Hide the hit marker
    }
}