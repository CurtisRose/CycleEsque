using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableController : MonoBehaviour
{
    [SerializeField] private SharedItemData consumableData;
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
					PlayerInventory.Instance.RemoveItemByID(consumableData.ID, 1);
					playerHealth.ReceiveHealing(((HealthItem)consumableData).HealingAmount);
				}
			}
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            SceneManagerHelper.LoadSceneWithPlayerData("SpaceStation");
        }
    }
}
