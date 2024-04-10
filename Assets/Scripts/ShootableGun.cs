using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableGun : WorldItem
{
    [SerializeField] Transform shootPositionTransform;
    [SerializeField] Projectile projectilePrefab;

    [SerializeField] float fireRate = 0.5f; // Time in seconds between shots
    private float lastShotTime = 0f; // Time since the last shot was fired

    [SerializeField] private float recoilAmountY = 5f; // How much the gun recoils
    [SerializeField] private float maxRecoilY = 20f; // Maximum recoil rotation on the x-axis
    [SerializeField] private float recoilAmountX = 2f; // How much the gun recoils sideways
    [SerializeField] private float maxRecoilX = 10f; // Maximum side-to-side recoil rotation
    [SerializeField] private float spreadAmount = 2f; // The variance in bullet direction

    private Quaternion originalRotation;
    private Quaternion targetRotation;
    [SerializeField] private float returnSpeed = 1f; // Speed at which the gun returns to original rotation



    private void Start()
    {
        originalRotation = transform.localRotation;
        targetRotation = originalRotation;
    }

    private void Update()
    {
        if (Character.disableUserInput)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            // Semi Auto
            Shoot();
        }

        if (Input.GetMouseButton(0))
        {
            // Fully Auto
            Shoot();
        }
    }

    void Shoot()
    {
        if (Time.time - lastShotTime < fireRate) return;

        if (Time.time - lastShotTime >= returnSpeed)
        {
            // Instantiate the projectile with no spread
            Projectile projectile = Instantiate<Projectile>(projectilePrefab, shootPositionTransform.position, shootPositionTransform.rotation);
        } else
        {
            // Calculate bullet spread
            Vector3 spread = Vector3.zero;
            spread += shootPositionTransform.up * Random.Range(-spreadAmount, spreadAmount);
            spread += shootPositionTransform.right * Random.Range(-spreadAmount, spreadAmount);

            Quaternion spreadRotation = Quaternion.Euler(spread) * shootPositionTransform.rotation;

            // Instantiate the projectile with spread applied
            Projectile projectile = Instantiate<Projectile>(projectilePrefab, shootPositionTransform.position, spreadRotation);
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
}
