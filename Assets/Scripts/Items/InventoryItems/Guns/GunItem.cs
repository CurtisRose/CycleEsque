using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Items/Gun")]
public class GunItem : BaseItem
{
    [field: SerializeField] public int MagazineCapacity { get; private set; }
    [field: SerializeField] public float RateOfFire { get; private set; }


    [field: SerializeField] public float recoilAmountY { get; private set; }
    [field: SerializeField] public float maxRecoilY { get; private set; }
    [field: SerializeField] public float recoilAmountX { get; private set; }
    [field: SerializeField] public float maxRecoilX { get; private set; }
    [field: SerializeField] public float spreadAmount { get; private set; }
}
