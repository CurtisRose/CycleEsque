using UnityEngine;
using UnityEngine.UI; // Required for interacting with UI elements

public class MonsterHealthUIController : MonoBehaviour
{
    [SerializeField] Slider healthSlider; // Reference to the UI Slider
    [SerializeField] Health targetHealth; // Reference to the Health component of your GameObject
    [SerializeField] Transform monsterTransform;

    [SerializeField] Vector3 offset = new Vector3(0, 2, 0);
    private Camera mainCamera;
    [SerializeField] RectTransform healthBarTransform;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Start()
    {
        healthSlider.maxValue = targetHealth.maxHealth;
        healthSlider.value = targetHealth.GetCurrentHealth();
    }

    void Update()
    {
		// TODO: This should NOT be being checked every frame, especially for large numbers of monsters
		if (targetHealth.ShouldDisplayHealthBar() && mainCamera != null) {
            // Convert the world position of the monster to a screen position and add an offset
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(monsterTransform.transform.position + offset);
            if (screenPosition.z > 0) // Check if the monster is in front of the camera
            {
                healthBarTransform.gameObject.SetActive(true);
                healthBarTransform.position = screenPosition;
            }
		} else {
			healthBarTransform.gameObject.SetActive(false); // Hide if target is behind the camera
		}
	}

    private void OnEnable()
    {
        // Subscribe to the health change event
        targetHealth.OnHealthChanged += UpdateHealthUI;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        targetHealth.OnHealthChanged -= UpdateHealthUI;
    }

    private void UpdateHealthUI(float currentHealth)
    {
        // Update the Slider to reflect current health
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
}