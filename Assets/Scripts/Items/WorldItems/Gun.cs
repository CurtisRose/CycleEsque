using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : WorldItem
{
    [SerializeField] Transform shootPositionTransform;
    [SerializeField] Transform aimPositionTransform;
    [SerializeField] Projectile projectilePrefab;
    [SerializeField] bool DrawGizmos;

    [SerializeField] float fireRate = 0.5f; // Time in seconds between shots
    private float lastShotTime = 0f; // Time since the last shot was fired

    private float recoilAmountY; // How much the gun recoils
    private float maxRecoilY; // Maximum recoil rotation on the x-axis
    private float recoilAmountX; // How much the gun recoils sideways
    private float maxRecoilX; // Maximum side-to-side recoil rotation
    private float spreadAmount; // The variance in bullet direction
    private float returnSpeed; // Speed at which the gun returns to original rotation

    [SerializeField] float damage;
    [SerializeField] float armorPenetration;

    private Quaternion originalRotation;
    private Quaternion targetRotation;

    [SerializeField] AudioSource gunAudioSource;
    [SerializeField] AudioClip weaponFireSound;
    [SerializeField] AudioClip weaponReloadSound;
    [SerializeField] AudioClip weaponEquipSound;

    int magazineCapacity;
    [SerializeField] int numberOfRounds;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        originalRotation = transform.localRotation;
        targetRotation = originalRotation;
    }

    protected override void InitializeItemFromBaseItemData()
    {
        base.InitializeItemFromBaseItemData();
        magazineCapacity = ((GunItem)sharedItemData).MagazineCapacity;
        fireRate = ((GunItem)sharedItemData).RateOfFire;
        recoilAmountY = ((GunItem)sharedItemData).recoilAmountY;
        maxRecoilY = ((GunItem)sharedItemData).maxRecoilY;
        recoilAmountX = ((GunItem)sharedItemData).recoilAmountX;
        maxRecoilX = ((GunItem)sharedItemData).maxRecoilX;
        spreadAmount = ((GunItem)sharedItemData).spreadAmount;
        returnSpeed = ((GunItem)sharedItemData).returnSpeed;
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

        Projectile projectileObj = ProjectilePool.Instance.GetProjectile();
        if (projectileObj != null)
        {
            projectileObj.SetInitialVisualPosition(aimPositionTransform.position.y - shootPositionTransform.position.y);

            projectileObj.transform.position = aimPositionTransform.position;
            projectileObj.transform.rotation = aimPositionTransform.rotation;
            projectileObj.gameObject.SetActive(true);

            /*
            if (Time.time - lastShotTime < returnSpeed)
            {
                ApplyBulletSpread(projectileObj.transform);
            }*/

            numberOfRounds--;
            gunAudioSource.PlayOneShot(weaponFireSound);
            lastShotTime = Time.time;
            return true;
        }

        return false;
    }

    void ApplyRecoil()
    {
        // Get current rotation in Euler angles for easy manipulation
        Vector3 currentEuler = targetRotation.eulerAngles;

        // Calculate the intended new Euler angles with recoil
        float intendedVerticalRecoil = currentEuler.x - recoilAmountY; // Assuming X is vertical
        float intendedHorizontalRecoil = currentEuler.y + Random.Range(-recoilAmountX, recoilAmountX); // Y is horizontal

        // Clamp vertical recoil
        if (Mathf.Abs(intendedVerticalRecoil - originalRotation.eulerAngles.x) > maxRecoilY)
        {
            intendedVerticalRecoil = originalRotation.eulerAngles.x - maxRecoilY * Mathf.Sign(recoilAmountY);
        }

        // Clamp horizontal recoil
        float horizontalDifference = Mathf.DeltaAngle(originalRotation.eulerAngles.y, intendedHorizontalRecoil);
        if (Mathf.Abs(horizontalDifference) > maxRecoilX)
        {
            intendedHorizontalRecoil = originalRotation.eulerAngles.y + maxRecoilX * Mathf.Sign(horizontalDifference);
        }

        // Apply the adjusted recoil to targetRotation
        targetRotation = Quaternion.Euler(intendedVerticalRecoil, intendedHorizontalRecoil, currentEuler.z);
        StopAllCoroutines(); // Stop any ongoing recoil correction
        StartCoroutine(RecoilCoroutine());
    }


    private void ApplyBulletSpread(Transform projectileTransform)
    {
        Vector3 spread = Vector3.zero;
        spread += projectileTransform.up * Random.Range(-spreadAmount, spreadAmount);
        spread += projectileTransform.right * Random.Range(-spreadAmount, spreadAmount);

        projectileTransform.rotation = Quaternion.Euler(spread) * projectileTransform.rotation;
    }



    IEnumerator RecoilCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fireRate)
        {
            // Move towards the target rotation
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, elapsedTime / fireRate);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Now, return to original rotation smoothly
        elapsedTime = 0f;
        while (transform.localRotation != originalRotation)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, elapsedTime * returnSpeed);
            targetRotation = transform.localRotation;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
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
}
