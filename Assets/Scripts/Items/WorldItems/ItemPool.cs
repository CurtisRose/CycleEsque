using UnityEngine;

[CreateAssetMenu(fileName = "NewItemPool", menuName = "Item Spawner/Item Pool", order = 1)]
public class ItemPool : ScriptableObject
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

    public ItemEntry[] items;

    public (WorldItem item, int quantity) GetRandomItemWithQuantity()
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
                int quantity = Random.Range(entry.minQuantity, entry.maxQuantity + 1);
                return (entry.item, quantity);
            }
            randomPoint -= entry.probability;
        }

        return (null, 0); // Should not happen, but just in case
    }
}