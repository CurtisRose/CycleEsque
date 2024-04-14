using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Helmet", menuName = "Items/Helmet")]
public class HelmetItem : SharedItemData
{
    [field: SerializeField] public float AmountOfArmor { get; private set; }
    [field: SerializeField] public int DefenseValue { get; private set; }
}
