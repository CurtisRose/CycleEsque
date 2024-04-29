using UnityEngine;
using UnityEngine.UI; // Required for interacting with UI elements

public class PlayerHealthUIController : MonoBehaviour
{
    [SerializeField] Slider healthSlider; // Reference to the UI Slider
    Health targetHealth; // Reference to the Health component of your GameObject

    private void Start()
    {
		targetHealth = PlayerWeaponController.Instance.gameObject.GetComponent<Health>();
		healthSlider.maxValue = targetHealth.maxHealth;
        healthSlider.value = targetHealth.GetCurrentHealth();
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