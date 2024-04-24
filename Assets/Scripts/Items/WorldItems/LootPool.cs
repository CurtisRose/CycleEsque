using UnityEngine;

[CreateAssetMenu(fileName = "NewLootPool", menuName = "LootPool", order = 1)]
public class LootPool : ScriptableObject
{
    [System.Serializable]
    public struct ItemEntry
    {
        public WorldItem item;
        public float probability; // The probability of this item being chosen
        [Range(1, 100)] // Adjust this range based on your needs
        public int minQuantity;
        [Range(1, 100)] // Ensure this is the same or higher than minQuantity
        public int maxQuantity;
	}

    [SerializeField] protected ItemEntry[] items;

    public WorldItem GetRandomItemWithQuantity()
    {
        float totalProbability = 0;
        foreach (ItemEntry entry in items)
        {
            totalProbability += entry.probability;
        }

        float randomPoint = Random.Range(0, totalProbability);

        foreach (ItemEntry entry in items)
        {
            if (randomPoint < entry.probability)
            {
                if (entry.item != null) {
                    int quantity = Random.Range(entry.minQuantity, entry.maxQuantity + 1);

                    if (entry.item.GetSharedItemData().Stackable) {
                        entry.item.SetNumberOfStartingItems(quantity);
                    } else {
                        entry.item.SetNumberOfStartingItems(1);
                    }

                    return (entry.item);
                } else
                    return null;
            }
            randomPoint -= entry.probability;
        }

        return (null); // Should not happen, but just in case
    }

    public int NumberOfItems()
    {
        return items.Length;
    }

	private void OnValidate() {
		for (int i = 0; i < items.Length; i++) {
			items[i].minQuantity = Mathf.Max(1, items[i].minQuantity);
			items[i].maxQuantity = Mathf.Max(items[i].minQuantity, items[i].maxQuantity);
		}
	}
}