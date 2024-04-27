using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Armor", menuName = "Items/Armor")]
public class ArmorItem : SharedItemData
{
    [field: SerializeField] public float AmountOfArmorMax { get; private set; }
    [field: SerializeField] public int ArmorValue { get; private set; }

    protected override void PopulateAllowedKeys()
    {
        base.PopulateAllowedKeys();

        allowedKeys.Add(ItemAttributeKeys.KeyToString(ItemAttributeKey.ArmorRemaining));
    }
}
