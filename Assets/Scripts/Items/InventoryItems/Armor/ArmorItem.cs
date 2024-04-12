using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Armor", menuName = "Items/Armor")]
public class ArmorItem : BaseItem
{
    [field: SerializeField] public float AmountOfArmor { get; private set; }
    [field: SerializeField] public int DefenseValue { get; private set; }
}
