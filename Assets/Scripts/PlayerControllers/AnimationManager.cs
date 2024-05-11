using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
	public static AnimationManager Instance;
	[SerializeField] private Animator playerAnimator;
	CharacterController characterController;
	float maxWalkingSpeed = 0.0f;
	float maxSprintSpeed = 0.0f;

	RuntimeAnimatorController defaultRuntimeAnimatorController;
	PlayerGearManager playerGearManager;

	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
		characterController = GetComponent<CharacterController>();
		defaultRuntimeAnimatorController = playerAnimator.runtimeAnimatorController;
		playerGearManager = GetComponent<PlayerGearManager>();
	}

	void Start() {
		maxWalkingSpeed = GetComponent<Player>().movementSpeed;
		maxSprintSpeed = GetComponent<Player>().sprintSpeed;
	}

	private void FixedUpdate() {
		if (ActionStateManager.Instance.IsWalking) {
			float normalizedSpeed = Mathf.Clamp01(characterController.velocity.magnitude / maxWalkingSpeed);
			playerAnimator.SetFloat("WalkingSpeed", normalizedSpeed);
		}
		if (ActionStateManager.Instance.IsRunning) {
			float normalizedSpeed = Mathf.Clamp01(characterController.velocity.magnitude / maxSprintSpeed);
			playerAnimator.SetFloat("RunningSpeed", normalizedSpeed);
		}
	}

	public void HandleAnimationCommand(ActionState command, bool active) {
		switch (command) {
			case ActionState.Running:
				playerAnimator.SetBool("IsRunning", active);
				if (active) {
					playerAnimator.SetFloat("RunningSpeed", 1f);
				} else {
					playerAnimator.SetFloat("RunningSpeed", 0f);
				}
				break;
			case ActionState.Walking:
				playerAnimator.SetBool("IsWalking", active);
				if (active) {
					float normalizedSpeed = Mathf.Clamp01(characterController.velocity.magnitude / maxWalkingSpeed);
					playerAnimator.SetFloat("WalkingSpeed", normalizedSpeed);
				} else {
					playerAnimator.SetFloat("WalkingSpeed", 0f);
				}
				break;
			case ActionState.Aiming:
				playerAnimator.SetBool("IsAiming", active);
				if (active) {
					CameraFOVController.Instance.SetFOV(50, 0.3f);
				} else {
					CameraFOVController.Instance.SetFOV(60, 0.2f);
				}
				break;
			case ActionState.Firing:
				if (active) {
					playerAnimator.ResetTrigger("IsFiring");
					playerAnimator.SetTrigger("IsFiring");
					playerGearManager.GetGunInHands().GetComponent<Animator>().SetTrigger("IsFiring");
				}
				break;
			case ActionState.Reloading:
				if (active) {
					playerAnimator.ResetTrigger("IsReloading");
					playerAnimator.SetTrigger("IsReloading");
					//playerGearManager.GetGunInHands().GetComponent<Animator>().SetTrigger("IsReloading");
				}
				break;
		}
	}

	public void SetAnimationOverrideController(AnimatorOverrideController overrideController) {
		if (overrideController != null) {
			playerAnimator.runtimeAnimatorController = overrideController;
		} else {
			playerAnimator.runtimeAnimatorController = defaultRuntimeAnimatorController;
		}
	}
}
