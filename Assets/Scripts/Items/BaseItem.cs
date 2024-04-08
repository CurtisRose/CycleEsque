using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Rarity { COMMON, UNCOMMON, RARE, EPIC, LEGENDARY, EXOTIC };
public enum ItemType { PRIMARY_WEAPON, HELMET, ARMOR, BACKPACK, OTHER };

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Items/BaseItem")]
public class BaseItem : ScriptableObject
{
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public ItemType ItemType { get; private set; }
    [field: SerializeField] public Rarity Rarity { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public float Weight { get; private set; }
    [field: SerializeField] public bool stackable { get; private set; }
    [field: SerializeField] public int maxStackSize { get; private set; }

}
