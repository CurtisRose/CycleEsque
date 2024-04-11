using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : WorldItem
{
    [SerializeField] Transform shootPositionTransform;
    [SerializeField] Projectile projectilePrefab;

    [SerializeField] float fireRate = 0.5f; // Time in seconds between shots
    private float lastShotTime = 0f; // Time since the last shot was fired

    private float recoilAmountY; // How much the gun recoils
    private float maxRecoilY; // Maximum recoil rotation on the x-axis
    private float recoilAmountX; // How much the gun recoils sideways
    private float maxRecoilX; // Maximum side-to-side recoil rotation
    private float spreadAmount; // The variance in bullet direction
    private float returnSpeed; // Speed at which the gun returns to original rotation

    private Quaternion originalRotation;
    private Quaternion targetRotation;

    int magazineCapacity;
    int numberOfRounds;

    protected override void Awake()
    {
        base.Awake();
        numberOfRounds = magazineCapacity;
    }

    protected override void Start()
    {
        base.Start();
        originalRotation = transform.localRotation;
        targetRotation = originalRotation;
    }

    protected override void InitializeItem()
    {
        base.InitializeItem();
        magazineCapacity = ((GunItem)item).MagazineCapacity;
        fireRate = ((GunItem)item).RateOfFire;
        recoilAmountY = ((GunItem)item).recoilAmountY;
        maxRecoilY = ((GunItem)item).maxRecoilY;
        recoilAmountX = ((GunItem)item).recoilAmountX;
        maxRecoilX = ((GunItem)item).maxRecoilX;
        spreadAmount = ((GunItem)item).spreadAmount;
        returnSpeed = ((GunItem)item).returnSpeed;
    }

    public override void Use()
    {
        Shoot();
    }

    public override void Equip()
    {
        base.Equip();
        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Gun"));
    }

    public override void Unequip()
    {
        base.Unequip();
        SetLayerRecursively(gameObject, LayerMask.NameToLayer("WorldItems"));
    }

    // Returns the number of rounds used
    public int Reload(int numRoundsAvailable)
    {
        int missingAmmoAmount = magazineCapacity - numberOfRounds;
        
        if (missingAmmoAmount > numRoundsAvailable)
        {
            numberOfRounds += numRoundsAvailable;
            return missingAmmoAmount;
        } else
        {
            numberOfRounds += missingAmmoAmount;
            return numRoundsAvailable - missingAmmoAmount;
        }
    }

    void Shoot()
    {
        if (Time.time - lastShotTime < fireRate) return;

        if (numberOfRounds <= 0) return;

        if (Time.time - lastShotTime >= returnSpeed)
        {
            // Instantiate the projectile with no spread
            Projectile projectile = Instantiate<Projectile>(projectilePrefab, shootPositionTransform.position, shootPositionTransform.rotation);
            numberOfRounds--;
        } else
        {
            // Calculate bullet spread
            Vector3 spread = Vector3.zero;
            spread += shootPositionTransform.up * Random.Range(-spreadAmount, spreadAmount);
            spread += shootPositionTransform.right * Random.Range(-spreadAmount, spreadAmount);

            Quaternion spreadRotation = Quaternion.Euler(spread) * shootPositionTransform.rotation;

            // Instantiate the projectile with spread applied
            Projectile projectile = Instantiate<Projectile>(projectilePrefab, shootPositionTransform.position, spreadRotation);
            numberOfRounds--;
        }

        ApplyRecoil();
        lastShotTime = Time.time;
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

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
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
}
