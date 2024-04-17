using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Items/Gun")]
public class GunItem : SharedItemData
{
    // Max number of rounds in magazine
    [field: SerializeField] public int MagazineCapacity { get; private set; }
    // Time in seconds between shots
    [field: SerializeField] public float RateOfFire { get; private set; }

    // Hipfire Recoil
    [field: SerializeField] public float hipFireMultiplier { get; private set; }

    // ADS Recoil
    [field: SerializeField] public float aimRecoilX { get; private set; }
    [field: SerializeField] public float aimRecoilY { get; private set; }
    [field: SerializeField] public float aimRecoilZ { get; private set; }

    // Settings
    [field: SerializeField] public float snappiness { get; private set; }
    [field: SerializeField] public float returnSpeed { get; private set; }

    // Gun Projectile Data
    [field: SerializeField] public float damage { get; private set; }
    [field: SerializeField] public float speed { get; private set; }
    [field: SerializeField] public float penetration { get; private set; }



    protected override void PopulateAllowedKeys()
    {
        base.PopulateAllowedKeys();

        allowedKeys.Add(ItemAttributeKeys.KeyToString(ItemAttributeKey.AmmoCount));
    }
}
