using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    private CharacterController characterController;

    [Header("Transforms")]
    [SerializeField] private Transform head;

    [Header("Movement Parameters")]
    [SerializeField] private float rotationSpeedHorizontal;
    [SerializeField] private float rotationSpeedVertical;
    [SerializeField] public float movementSpeed;
    [SerializeField] public float sprintSpeed;
    [SerializeField] private float crouchSpeed;
    private float currentMovementSpeed = 0.0f;
    private float headRotation = 0.0f;
    [SerializeField] public static bool disableUserMovementInputStatus = false;
    [SerializeField] public static bool disableUserClickingInputStatus = false;
    [SerializeField] public static bool disableUserLookingInputStatus = false;

    [Header("Jump Parameters")]
    // Adds forgiveness to jumps, allows you to fall off an edge for a short amount of time and still be able to jump.
    [SerializeField] private float jumpAidTime;
    // Dicates how soon after jumping that you can jump again. Must be grounded though. Prevents double jumping.
    [SerializeField] private float jumpTimerLimit;
    [SerializeField] public float jumpSpeed;
    [SerializeField] private float gravity;
    private float currentVerticalSpeed = 0.0f;
    private bool jumpedRecently = false;
    private float jumpTimer = 0.0f;
    private float inAirTime = 0.0f;

    [Header("Cameras")]
    [SerializeField] private Camera firstPersonCamera;
    [SerializeField] private Camera thirdPersonCamera;

    [SerializeField] float walkingSlopeLimit;
    [SerializeField] float sprintingSlopeLimit;
    float currentSlopeLimit;

    bool isCrouched;
    bool isMoving;
    bool isSprinting;


    private void Awake()
    {
        if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
        characterController = GetComponent<CharacterController>();
        this.jumpTimer = 0;
        SetUserMovementInputStatus(true);
    }

    private void Update()
    {
        if (!disableUserLookingInputStatus)
        {
            transform.Rotate(0, Input.GetAxisRaw("Mouse X") * rotationSpeedHorizontal, 0);

            //Rotate head but clamp it between -90 and 90 degrees
            headRotation += Input.GetAxis("Mouse Y") * rotationSpeedVertical * -1;
            headRotation = Mathf.Clamp(headRotation, -90.0f, 90.0f);
            head.localEulerAngles = new Vector3(headRotation, head.localEulerAngles.y, head.localEulerAngles.z);
        }

        if (!disableUserMovementInputStatus)
        {
            HandleJumpInput();

            currentMovementSpeed = movementSpeed;
            isCrouched = false;
            isSprinting = false;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentMovementSpeed = sprintSpeed;
                isSprinting = true;
                if (currentSlopeLimit != sprintingSlopeLimit)
                {
                    currentSlopeLimit = sprintingSlopeLimit;
                    characterController.slopeLimit = currentSlopeLimit;
                }
            } else
            {
                if (currentSlopeLimit != walkingSlopeLimit)
                {
                    currentSlopeLimit = walkingSlopeLimit;
                    characterController.slopeLimit = currentSlopeLimit;
                }
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                isCrouched = true;
                currentMovementSpeed = crouchSpeed;
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 move = Vector3.zero;
        isMoving = false;
		PlayerNoiseLevel noiseLevel = PlayerNoiseLevel.None;

		if (!disableUserMovementInputStatus)
        {
			// Get the input vector from keyboard or controller
			Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

			if (inputDirection.magnitude > 1) {
				inputDirection.Normalize();
			}

			move = (transform.forward * Input.GetAxis("Vertical") * currentMovementSpeed * Time.deltaTime +
					transform.right * Input.GetAxis("Horizontal") * currentMovementSpeed * Time.deltaTime);

			if (isSprinting) {
				noiseLevel = PlayerNoiseLevel.Medium; // Running noise
			} else if (isCrouched) {
				noiseLevel = PlayerNoiseLevel.None; // Crouching noise
			} else if (move.magnitude > 0) {
				noiseLevel = PlayerNoiseLevel.Low; // Walking noise
			}
		}
        if (move.magnitude > 0)
        {
            isMoving = true;
			PlayerSoundController.Instance.RegisterSound(noiseLevel, transform.position);
            if (isSprinting) {
				ActionStateManager.Instance.TrySetRunning(true);
			} else {
				if (ActionStateManager.Instance.IsRunning) {
					ActionStateManager.Instance.TrySetRunning(false);
				}
				ActionStateManager.Instance.TrySetWalking(true);
			}
		} else {
            // TODO: Maybe let the animation manager set these false when the speed falls.
			ActionStateManager.Instance.TrySetWalking(false);
			ActionStateManager.Instance.TrySetRunning(false);
		}
        currentVerticalSpeed -= gravity * Time.deltaTime;
        move.y = currentVerticalSpeed;
        characterController.Move(move);
    }

    private void HandleJumpInput()
    {
        // If you are grounded and have not jumped recently
        if (characterController.isGrounded && !jumpedRecently)
        {
            inAirTime = 0.0f;
            currentVerticalSpeed = 0.0f;
            if (Input.GetKey(KeyCode.Space))
            {
                jumpedRecently = true;
                currentVerticalSpeed = jumpSpeed;
				PlayerSoundController.Instance.RegisterSound(PlayerNoiseLevel.High, transform.position);
			}
        }
        else if (!characterController.isGrounded && !jumpedRecently)
        {
            if (inAirTime < jumpAidTime)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    jumpedRecently = true;
                    currentVerticalSpeed = jumpSpeed;
					PlayerSoundController.Instance.RegisterSound(PlayerNoiseLevel.High, transform.position);
				}
			}
            inAirTime += Time.deltaTime;
        }

        if (jumpedRecently)
        {
            if (jumpTimer >= jumpTimerLimit)
            {
                jumpedRecently = false;
                jumpTimer = 0.0f;
            }
            else
            {
                jumpTimer += Time.deltaTime;
            }
        }
    }

    private void SwitchPerspective()
    {
        firstPersonCamera.gameObject.SetActive(!firstPersonCamera.gameObject.activeSelf);
        thirdPersonCamera.gameObject.SetActive(!thirdPersonCamera.gameObject.activeSelf);
    }

    public static void SetUserMovementInputStatus(bool enabled)
    {
        if (enabled)
        {
            disableUserMovementInputStatus = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            disableUserMovementInputStatus = true;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }

    public static void SetUserLookingInputStatus(bool enabled)
    {
        if (enabled)
        {
            disableUserLookingInputStatus = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            disableUserLookingInputStatus = true;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }

    public static void SetUserClickingInputStatus(bool enabled)
    {
        if (enabled)
        {
            disableUserClickingInputStatus = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            disableUserClickingInputStatus = true;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }

    public bool GetIsCrouched()
    {
        return isCrouched;
    }

    public bool GetIsJumping()
    {
        return !characterController.isGrounded;
    }

    public bool GetIsSprinting()
    {
        return isSprinting && !GetIsCrouched();
    }

    public bool GetIsMoving()
    {
        return isMoving;
    }
}
