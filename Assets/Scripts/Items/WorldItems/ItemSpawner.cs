using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public ItemPool itemPool;
    public float respawnTime; // Time in seconds to respawn an item

    private WorldItem currentItem = null;
    private float timer = 0;

    private void Start()
    {
        SpawnItem(); // Attempt to spawn an item right away
    }

    public void CheckRespawn()
    {
        if (currentItem == null)
        {
            timer += Time.deltaTime;
            if (timer >= respawnTime)
            {
                SpawnItem();
                timer = 0;
            }
        }
    }

    private void SpawnItem()
    {
        var (selectedItem, quantity) = itemPool.GetRandomItemWithQuantity();

        if (selectedItem != null && quantity > 0)
        {
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            WorldItem spawnedItem = Instantiate(selectedItem, transform.position, randomRotation, transform);
            
            if (spawnedItem.GetComponent<WorldItem>() != null)
            {
                if (spawnedItem.GetBaseItem().stackable)
                {
                    spawnedItem.SetNumberOfStartingItems(quantity);
                }
                spawnedItem.GetComponent<WorldItem>().OnPickedUp += ItemTaken;
            }
        }
        else
        {
            //Debug.Log("No item spawned this time.");
            currentItem = null;
        }
    }

    private void ItemTaken()
    {
        currentItem = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 1.0f);
    }
}