using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Items/Weapons/BaseWeapon")]
public class WeaponItem : BaseItem
{
    [field: SerializeField] public Sprite LargeImage { get; private set; }
}
