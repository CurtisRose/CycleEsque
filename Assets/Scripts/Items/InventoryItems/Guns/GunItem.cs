using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Items/Gun")]
public class GunItem : BaseItem
{
    // Max number of rounds in magazine
    [field: SerializeField] public int MagazineCapacity { get; private set; }
    // Time in seconds between shots
    [field: SerializeField] public float RateOfFire { get; private set; }


    // How much the gun recoils on the Y-Axis
    [field: SerializeField] public float recoilAmountY { get; private set; }
    // Maximum recoil on the Y-Axis
    [field: SerializeField] public float maxRecoilY { get; private set; }
    // How much the gun recoils on the X-Axis
    [field: SerializeField] public float recoilAmountX { get; private set; }
    // Maximum recoil on the X-Axis
    [field: SerializeField] public float maxRecoilX { get; private set; }
    // The variance in bullet direction
    [field: SerializeField] public float spreadAmount { get; private set; }
    // Speed at which the gun returns to original rotation
    [field: SerializeField] public float returnSpeed { get; private set; }
}
