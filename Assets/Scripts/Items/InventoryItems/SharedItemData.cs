using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Rarity { COMMON, UNCOMMON, RARE, EPIC, LEGENDARY, EXOTIC };
public enum ItemType { WEAPON, HELMET, ARMOR, BACKPACK, AMMO, OTHER };

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Items/BaseItem")]
public class SharedItemData : ScriptableObject
{
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public string ID { get; private set; } = System.Guid.NewGuid().ToString();
    [field: SerializeField] public string ItemDescription { get; private set; }
    [field: SerializeField] public ItemType ItemType { get; private set; }
    [field: SerializeField] public Rarity Rarity { get; private set; }
    [field: SerializeField] public Sprite SmallImage { get; private set; }
    // Only Used For Weapons for now
    [field: SerializeField] public Sprite LargeImage { get; private set; }
    [field: SerializeField] public float Weight { get; private set; }
    [field: SerializeField] public bool Stackable { get; private set; }
    [field: SerializeField] public int MaxStackSize { get; private set; }

    [field: SerializeField] public bool ColorGameObjectBasedOnRarity { get; private set; }

    public List<string> allowedKeys = new List<string>();

    protected virtual void OnValidate()
    {
        allowedKeys.Clear();
        PopulateAllowedKeys();
    }

    protected virtual void PopulateAllowedKeys()
    {
        allowedKeys.Add(ItemAttributeKeys.KeyToString(ItemAttributeKey.NumItemsInStack));
    }
}
