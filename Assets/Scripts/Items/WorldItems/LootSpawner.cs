using UnityEngine;

public class LootSpawner : MonoBehaviour, IActivatable
{
    public LootPool itemPool;
    public float respawnTime; // Time in seconds to respawn an item

    private WorldItem currentItem = null;
    bool isActive;

	public void Activate() {
        if (isActive) {
			return;
		}
        isActive = true;
		SpawnItem();
	}

    public void Deactivate() {
        if (!isActive) {
			return;
		}
        isActive = false;
        if (currentItem != null) {
            Destroy(currentItem.gameObject);
        }
	}

	public bool IsActive() {
		return isActive;
	}

    private void SpawnItem()
    {
        WorldItem selectedItemPrefab = itemPool.GetRandomItemWithQuantity();

        if (selectedItemPrefab != null && selectedItemPrefab.GetNumberOfItems() > 0)
        {
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
			currentItem = Instantiate(selectedItemPrefab, transform.position, randomRotation, transform);
            
            if (currentItem.GetComponent<WorldItem>() != null)
            {
                if (currentItem.GetSharedItemData().Stackable)
                {
					currentItem.SetNumberOfStartingItems(selectedItemPrefab.GetNumberOfItems());
                }
				//currentItem.GetComponent<WorldItem>().OnPickedUp += ItemTaken;
            }
        }
        else
        {
            //Debug.Log("No item spawned this time.");
            currentItem = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 1.0f);
    }
}