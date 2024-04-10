using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Backpack", menuName = "Items/Backpack")]
public class BackpackItem : BaseItem
{
    [field: SerializeField] public float CarryCapacity { get; private set; }
}
