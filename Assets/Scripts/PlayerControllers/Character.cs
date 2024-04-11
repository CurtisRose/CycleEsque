using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterController characterController;

    [Header("Transforms")]
    [SerializeField] private Transform head;

    [Header("Movement Parameters")]
    [SerializeField] private float rotationSpeedHorizontal;
    [SerializeField] private float rotationSpeedVertical;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float crouchSpeed;
    private float currentMovementSpeed = 0.0f;
    private float headRotation = 0.0f;
    [SerializeField] public static bool disableUserInput = false;
    
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


    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        this.jumpTimer = 0;
        SetUserInputStatus(true);
    }

    private void Update()
    {
        if (disableUserInput)
        {
            return;
        }
        transform.Rotate(0, Input.GetAxisRaw("Mouse X") * rotationSpeedHorizontal, 0);

        //Rotate head but clamp it between -90 and 90 degrees
        headRotation += Input.GetAxis("Mouse Y") * rotationSpeedVertical * -1;
        headRotation = Mathf.Clamp(headRotation, -90.0f, 90.0f);
        head.localEulerAngles = new Vector3(headRotation, head.localEulerAngles.y, head.localEulerAngles.z);


        if (Input.GetKeyDown(KeyCode.P))
        {
            SwitchPerspective();
        }

        HandleJumpInput();

        currentMovementSpeed = movementSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentMovementSpeed = sprintSpeed;
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

        if (!disableUserInput)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                currentMovementSpeed = crouchSpeed;
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 move = Vector3.zero;

        if (!disableUserInput)
        {
            move = (transform.forward * Input.GetAxis("Vertical") * currentMovementSpeed * Time.deltaTime +
                    transform.right * Input.GetAxis("Horizontal") * currentMovementSpeed * Time.deltaTime);
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

    public static void SetUserInputStatus(bool enabled)
    {
        if (enabled)
        {
            disableUserInput = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else
        {
            disableUserInput = true;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
}
