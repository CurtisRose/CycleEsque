using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoil : MonoBehaviour
{
    PlayerGearController playerGearController;
    Character character;

    [SerializeField] Transform recoilTarget;
    bool isAiming;
    [SerializeField] float aimMultiplier = 1f;

    [SerializeField] float crouchMultiplier;
    [SerializeField] float sprintMultiplier;
    [SerializeField] float jumpMultiplier;
    [SerializeField] float moveMultiplier;

    // Rotations
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    // Hipfire Recoil
    [SerializeField] private float hipFireMultiplier;

    // ADS Recoil
    [SerializeField] private float aimRecoilX;
    [SerializeField] private float aimRecoilY;
    [SerializeField] private float aimRecoilZ;

    // Settings
    [SerializeField] private float snappiness = 2;
    [SerializeField] private float returnSpeed = 2;

    GunSharedItemData gunData;
    protected void InitializeItemFromBaseItemData(GunSharedItemData gunData)
    {
        if (gunData == null) return;
        this.gunData = gunData;
        // Hipfire Recoil
        this.hipFireMultiplier = gunData.hipFireMultiplier;

        // ADS Recoil
        this.aimRecoilX = gunData.aimRecoilX;
        this.aimRecoilY = gunData.aimRecoilY;
        this.aimRecoilZ = gunData.aimRecoilZ;

        // Settings
        this.snappiness = gunData.snappiness;
        this.returnSpeed = gunData.returnSpeed;
    }

    private void Awake()
    {
        playerGearController = GetComponent<PlayerGearController>();
        character = GetComponent<Character>();
        playerGearController.OnPrimaryChanged += InitializeItemFromBaseItemData;
    }

    private void Update()
    {
        isAiming = playerGearController.IsADSing();

        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        recoilTarget.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire()
    {
        float movementMultiplier = 1f;
        if (character.GetIsCrouched())
        {
            movementMultiplier *= 0.5f;  // Assume a crouch reduces recoil impact by half
        }

        if (character.GetIsMoving())
        {
            if (character.GetIsSprinting())
            {
                movementMultiplier *= 1.2f;  // Sprinting slightly increases recoil
            }
            if (character.GetIsJumping())
            {
                movementMultiplier *= 1.5f;  // Jumping significantly increases recoil
            }
        }

        // Determine the base recoil values
        Vector3 baseRecoil = new Vector3(aimRecoilX, aimRecoilY, aimRecoilZ);

        // Apply the hip fire multiplier if not aiming
        float finalMultiplier = isAiming ? 1f : hipFireMultiplier;

        // Calculate the final recoil with randomness and movement adjustments
        Vector3 finalRecoil = new Vector3(
            -baseRecoil.x,
            Random.Range(-baseRecoil.y, baseRecoil.y),
            Random.Range(-baseRecoil.z, baseRecoil.z)) * finalMultiplier * movementMultiplier;

        // Apply the calculated recoil to the target rotation
        targetRotation += finalRecoil;
    }
}

