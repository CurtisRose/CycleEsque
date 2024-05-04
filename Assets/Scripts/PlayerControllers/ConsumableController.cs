using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ConsumableController : MonoBehaviour
{
    [SerializeField] private HealthItem consumableData;
    [SerializeField] private PlayerHealth playerHealth;

	[SerializeField] float longPressThreshold;  // Duration threshold to define a long press
	[SerializeField] bool isPressed = false;
	[SerializeField] float pressTime = 0;

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.E)) {
			isPressed = true;
			pressTime = Time.time;  // Record the time when 'E' was pressed
		}

		// Short press action, use consumable
		if (Input.GetKeyUp(KeyCode.E)) {
			if (ConsumableSelectionMenu.Instance.IsOpen()) {
				ConsumableSelectionMenu.Instance.Close();
			}

			isPressed = false;
			if (Time.time - pressTime < longPressThreshold) {
				UseConsumable();
			}
		}

		// Long press action, open consumable context window
		if (isPressed && (Time.time - pressTime > longPressThreshold)) {
			OpenContextWindow();
		}
    }

	private void UseConsumable() {
		int numConsumables = PlayerInventory.Instance.GetNumberOfItems(consumableData.ID);
		if (numConsumables > 0) {
			if (playerHealth.GetMissingHealth() > 0) {
				if (!ActionStateManager.Instance.CanPerformAction(ActionState.UsingConsumable)) return;
				ActionStateManager.Instance.EnterState(ActionState.UsingConsumable);
				Invoke("ExitUsingConsumableState", consumableData.TimeToUse);
			}
		}
	}

	private void ExitUsingConsumableState() {
		PlayerInventory.Instance.RemoveItemByID(consumableData.ID, 1);
		playerHealth.ReceiveHealing(((HealthItem)consumableData).HealingAmount);
		ActionStateManager.Instance.ExitState(ActionState.UsingConsumable);
	}

	private void OpenContextWindow() {
		if (!ConsumableSelectionMenu.Instance.IsOpen())
			ConsumableSelectionMenu.Instance.Open();
	}


	public void SetConsumableToUse(HealthItem itemData) {
		consumableData = itemData;
	}
}
