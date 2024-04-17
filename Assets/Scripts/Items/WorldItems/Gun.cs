using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : WorldItem
{
    [SerializeField] Transform shootPositionTransform;
    [SerializeField] Transform aimPositionTransform;
    [SerializeField] Projectile projectilePrefab;
    [SerializeField] bool DrawGizmos;

    float projectileSpeed = 100f;
    float projectileDamage = 25f;
    float projectileArmorPenetration = 0.5f;
    float fireRate = 0.5f; // Time in seconds between shots
    private float lastShotTime = 0f; // Time since the last shot was fired

    [SerializeField] AudioSource gunAudioSource;
    [SerializeField] AudioClip weaponFireSound;
    [SerializeField] AudioClip weaponReloadSound;
    [SerializeField] AudioClip weaponEquipSound;

    int magazineCapacity;
    [SerializeField] int numberOfRounds;

    [SerializeField] Vector3 EquipPosition;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void InitializeItemFromBaseItemData()
    {
        base.InitializeItemFromBaseItemData();
        magazineCapacity = ((GunSharedItemData)sharedItemData).MagazineCapacity;
        fireRate = ((GunSharedItemData)sharedItemData).RateOfFire;
        projectileSpeed = ((GunSharedItemData)sharedItemData).speed;
        projectileArmorPenetration = ((GunSharedItemData)sharedItemData).penetration;
        projectileDamage = ((GunSharedItemData)sharedItemData).damage;
    }

    public override ItemInstance CreateItemInstance()
    {
        ItemInstance itemInstance = base.CreateItemInstance();
        itemInstance.SetProperty(ItemAttributeKey.AmmoCount, numberOfRounds);
        return itemInstance;
    }

    public override void InitializeFromItemInstance(ItemInstance instance)
    {
        base.InitializeFromItemInstance(instance);
        int ammoCount = (int)instance.GetProperty(ItemAttributeKey.AmmoCount);
        if (ammoCount > 0)
        {
            numberOfRounds = (int)ammoCount;
        } else
        {
            numberOfRounds = 0;
        }
    }

    public override ItemInstance CreateNewItemInstance(SharedItemData sharedData)
    {
        ItemInstance instance = base.CreateNewItemInstance(sharedData);
        instance.SetProperty(ItemAttributeKey.AmmoCount, 0);
        return instance;
    }

    public override bool Use()
    {
        return Shoot();
    }

    public override void Equip()
    {
        base.Equip();
        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Gun"));
        gunAudioSource.PlayOneShot(weaponEquipSound);
        transform.localPosition = EquipPosition;
    }

    public void PlayWeaponSwapSound()
    {
        gunAudioSource.PlayOneShot(weaponEquipSound);
    }

    // Returns the number of rounds used
    public int Reload(int numRoundsAvailable)
    {
        if (numRoundsAvailable <= 0)
        {
            return 0;
        }

        int missingAmmoAmount = magazineCapacity - numberOfRounds;
        if (missingAmmoAmount <= 0)
        {
            return 0;
        }

        gunAudioSource.PlayOneShot(weaponReloadSound);

        if (missingAmmoAmount > numRoundsAvailable)
        {
            numberOfRounds += numRoundsAvailable;
            return numRoundsAvailable;
        } else
        {
            numberOfRounds += missingAmmoAmount;
            return missingAmmoAmount;
        }
    }

    bool Shoot()
    {
        if (Time.time - lastShotTime < fireRate) return false;

        if (numberOfRounds <= 0) return false;

        Debug.DrawRay(aimPositionTransform.position, aimPositionTransform.forward * 10, Color.red, 2.0f);

        Projectile projectile = ProjectilePool.Instance.GetProjectile();

        if (projectile  != null)
        {
            projectile.SetInitialVisualPosition(aimPositionTransform.position.y - shootPositionTransform.position.y);

            projectile.transform.position = aimPositionTransform.position;
            projectile.transform.rotation = aimPositionTransform.rotation;

            projectile.Speed = projectileSpeed;
            projectile.Damage = projectileDamage;
            projectile.ArmorPenetration = projectileArmorPenetration;

            projectile.gameObject.SetActive(true);

            numberOfRounds--;
            gunAudioSource.PlayOneShot(weaponFireSound);
            lastShotTime = Time.time;
            return true;
        }

        return false;
    }

    public int GetMagazineCapacity()
    {
        return magazineCapacity;
    }

    public int GetNumberOfRounds()
    {
        return numberOfRounds;
    }

    public Transform GetGunAimPosition()
    {
        return shootPositionTransform;
    }

    void OnDrawGizmos()
    {
        if (DrawGizmos)
        {
            if (shootPositionTransform != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(shootPositionTransform.position, shootPositionTransform.forward * 500); // Draw a 2-meter long red ray
            }

            if (aimPositionTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(aimPositionTransform.position, aimPositionTransform.forward * 500); // Draw a 2-meter long green ray
            }
        }
    }

    public GunSharedItemData GetGunData()
    {
        return (GunSharedItemData)sharedItemData;
    }
}
