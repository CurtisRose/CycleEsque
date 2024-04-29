using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableController : MonoBehaviour
{
    [SerializeField] private HealthItem consumableData;
    [SerializeField] private PlayerHealth playerHealth;

    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) {
            int numConsumables = PlayerInventory.Instance.GetNumberOfItems(consumableData.ID);
            if (numConsumables > 0) {
                if (playerHealth.GetMissingHealth() > 0) {
					if (!ActionStateManager.Instance.CanPerformAction(ActionState.UsingConsumable)) return;
					ActionStateManager.Instance.EnterState(ActionState.UsingConsumable);
					Invoke("ExitUsingConsumableState", consumableData.TimeToUse);
				}
			}
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            SceneManagerHelper.LoadSceneWithPlayerData("SpaceStation");
        }
    }

	private void ExitUsingConsumableState() {
		PlayerInventory.Instance.RemoveItemByID(consumableData.ID, 1);
		playerHealth.ReceiveHealing(((HealthItem)consumableData).HealingAmount);
		ActionStateManager.Instance.ExitState(ActionState.UsingConsumable);
	}
}
