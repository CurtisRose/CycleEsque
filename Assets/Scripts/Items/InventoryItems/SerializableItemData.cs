[System.Serializable]
public class SerializableItemData
{
    public string DisplayName;
    public string ID;
    public string ItemDescription;
    public ItemType ItemType; // Enums are serializable by default
    public Rarity Rarity;
    public float Weight;
    public bool Stackable;
    public int MaxStackSize;
    public int Quantity;

    // Transforming from SharedItemData to SerializableItemData
    public static SerializableItemData FromSharedItemData(SharedItemData sharedItem, int quantity)
    {
        return new SerializableItemData
        {
            ID = sharedItem.ID,
            DisplayName = sharedItem.DisplayName,
            ItemDescription = sharedItem.ItemDescription,
            ItemType = sharedItem.ItemType,
            Rarity = sharedItem.Rarity,
            Weight = sharedItem.Weight,
            Stackable = sharedItem.Stackable,
            MaxStackSize = sharedItem.MaxStackSize,
            Quantity = quantity
        };
    }
}
