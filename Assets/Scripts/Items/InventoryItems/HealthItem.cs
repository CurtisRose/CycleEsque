using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Injector", menuName = "Items/Injector")]
public class HealthItem : SharedItemData 
{
	[field: SerializeField] public float TimeToUse { get; private set; }
	[field: SerializeField] public int HealingAmount { get; private set; }
}
