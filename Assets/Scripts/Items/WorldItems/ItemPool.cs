using UnityEngine;

[CreateAssetMenu(fileName = "NewItemPool", menuName = "Item Spawner/Item Pool", order = 1)]
public class ItemPool : ScriptableObject
{
    [System.Serializable]
    public struct ItemEntry
    {
        public WorldItem item;
        public float probability; // The probability of this item being chosen
    }

    public ItemEntry[] items;

    public WorldItem GetRandomItem()
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
                return entry.item;
            }
            randomPoint -= entry.probability;
        }

        return null; // Should not happen, but just in case
    }
}