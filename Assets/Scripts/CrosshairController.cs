using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] RectTransform crossHairRoot;
    public List<RectTransform> indicators; // UI element's RectTransform
    public List<Vector2> movementDirections; // Directions for each indicator
    public float riseAmount = 10f; // Amount to rise with each click
    public float riseSpeed = 20f; // Speed at which the indicator tries to reach the target value
    public float fallSpeed = 5f; // Speed at which the target value falls
    public float indicatorMaxHeight = 100f; // Maximum height the indicator can reach
    public float targetMaxHeight; // Maximum target height, slightly more than the indicator's max height
    public float originalY; // Original starting y position
    public float targetValue; // The target position value
    public float currentValue; // The current position value
    public float noHitZeroingPoint = 50f;

    public Vector2 currentPosition;

    Vector3 velocity;

    void Start()
    {
        if (crossHairRoot == null || indicators == null || indicators.Count == 0)
        {
            Debug.LogError("Indicator RectTransform is not set on " + gameObject.name);
            this.enabled = false;
            return;
        }
        originalY = indicators[0].anchoredPosition.y;
        currentValue = originalY;
        targetValue = originalY;
        // I want maxHeight relative to the starting height
        indicatorMaxHeight += originalY;
        targetMaxHeight += originalY;
    }

    void Update()
    {
        currentPosition = indicators[0].anchoredPosition;

        // Gradually decrease the target value
        targetValue -= fallSpeed * Time.deltaTime;
        targetValue = Mathf.Max(targetValue, originalY); // Ensure target value doesn't go below originalY
        targetValue = Mathf.Min(targetValue, targetMaxHeight); // Ensure target value doesn't go above max height

        // Move the current value towards the target value
        currentValue = Mathf.MoveTowards(currentValue, targetValue, riseSpeed * Time.deltaTime);
        currentValue = Mathf.Min(currentValue, indicatorMaxHeight); // Ensure current value doesn't exceed the indicator's max height

        // Apply the current value to the indicator's position based on its direction
        for (int i = 0; i < indicators.Count; i++)
        {
            Vector2 direction = movementDirections[i];
            indicators[i].anchoredPosition += direction * (currentValue - indicators[i].anchoredPosition.magnitude);
        }
    }

    public void Bloom()
    {
        targetValue += riseAmount;
        targetValue = Mathf.Min(targetValue, targetMaxHeight); // Cap the target value at maxHeight
    }

    public void SetCrosshairPositionWhereGunIsLooking(Transform aimPosition, float smoothTime)
    {
        RaycastHit hit;
        // Define a layer mask that includes all layers except 'Projectiles'
        int layerMask = 1 << LayerMask.NameToLayer("Projectile");
        layerMask = ~layerMask; // Bitwise invert to ignore the 'Projectiles' layer

        Vector3 targetPoint;

        if (Physics.Raycast(aimPosition.position, aimPosition.forward, out hit, Mathf.Infinity, layerMask))
        {
            // If the raycast hits, use the hit point
            targetPoint = hit.point;
        }
        else
        {
            // If the raycast fails, use a point 50 units in front of the aim position
            targetPoint = aimPosition.position + aimPosition.forward * noHitZeroingPoint;
        }

        // Convert the target point from world space to screen space
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(targetPoint);
        // Smoothly move the crosshair to the target screen point
        crossHairRoot.transform.position = Vector3.SmoothDamp(crossHairRoot.transform.position, screenPoint, ref velocity, smoothTime);
    }
}
