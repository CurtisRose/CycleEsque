using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour
{
    [SerializeField] Collider lootCollider;
    [SerializeField] float moveSpeed;

    [SerializeField] private Item item;

    public void Initialize(Item item)
    {
        this.item = item;
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bool canAdd = InventoryManager.instance.AddItem(item);
            if (canAdd)
            {
                StartCoroutine(MoveAndCollect(other.transform));
            }
        }
    }*/

    private IEnumerator MoveAndCollect(Transform target)
    {
        Destroy(lootCollider);

        while(Vector3.Magnitude(transform.position-target.position) > 0.25f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            yield return 0;
        }

        Destroy(gameObject);
    }
}
