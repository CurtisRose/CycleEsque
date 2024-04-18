using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class HitMarker : MonoBehaviour
{
    public static HitMarker Instance { get; private set; }
    [SerializeField] GameObject hitMarkerObject;
    [SerializeField] float duration;  // Duration for how long the hit marker should be visible
    [SerializeField] float flashSpeed;
    bool active;
    [SerializeField] private List<Image> hitMarkerRenderers;

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

    public void ShowHitMarker(float criticalMultiplier)
    {
        // Calculate the color based on criticalMultiplier
        Color targetColor = GetColorBasedOnMultiplier(criticalMultiplier);
        foreach(Image image in hitMarkerRenderers)
        {
            image.color = targetColor;  // Set the color
        }

        if (active)
        {
            // If already active, flash off for a moment and then resume being on
            StopAllCoroutines();  // Stop any running coroutine to handle a new flash
            StartCoroutine(FlashHitMarker(targetColor));
        }
        else
        {
            // Else, activate as normal
            active = true;
            hitMarkerObject.SetActive(true);
            StartCoroutine(DeactivateAfterDelay());
        }
    }

    private IEnumerator FlashHitMarker(Color color)
    {
        hitMarkerObject.SetActive(false);
        yield return new WaitForSeconds(flashSpeed);  // Wait for the off-flash duration
        foreach (Image image in hitMarkerRenderers)
        {
            image.color = color;  // Set the color
        }
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

    private Color GetColorBasedOnMultiplier(float multiplier)
    {
        if (multiplier < 1.0f)
        {
            // Lerp between black and white
            return Color.Lerp(Color.black, Color.white, multiplier);
        }
        else
        {
            // Lerp between white and red
            return Color.Lerp(Color.white, Color.red, multiplier - 1.0f);
        }
    }
}