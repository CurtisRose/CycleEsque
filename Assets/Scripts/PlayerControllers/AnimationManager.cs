using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
	public static AnimationManager Instance;
	[SerializeField] private Animator animator;
	CharacterController characterController;
	float maxWalkingSpeed = 0.0f;
	float maxSprintSpeed = 0.0f;

	RuntimeAnimatorController defaultRuntimeAnimatorController;

	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
		characterController = GetComponent<CharacterController>();
		defaultRuntimeAnimatorController = animator.runtimeAnimatorController;
	}

	void Start() {
		maxWalkingSpeed = GetComponent<Player>().movementSpeed;
		maxSprintSpeed = GetComponent<Player>().sprintSpeed;
	}

	private void FixedUpdate() {
		if (ActionStateManager.Instance.IsWalking) {
			float normalizedSpeed = Mathf.Clamp01(characterController.velocity.magnitude / maxWalkingSpeed);
			animator.SetFloat("WalkingSpeed", normalizedSpeed);
		}
		if (ActionStateManager.Instance.IsRunning) {
			float normalizedSpeed = Mathf.Clamp01(characterController.velocity.magnitude / maxSprintSpeed);
			animator.SetFloat("RunningSpeed", normalizedSpeed);
		}
	}

	public void HandleAnimationCommand(ActionState command, bool active) {
		switch (command) {
			case ActionState.Running:
				animator.SetBool("IsRunning", active);
				if (active) {
					animator.SetFloat("RunningSpeed", 1f);
				} else {
					animator.SetFloat("RunningSpeed", 0f);
				}
				break;
			case ActionState.Walking:
				animator.SetBool("IsWalking", active);
				if (active) {
					float normalizedSpeed = Mathf.Clamp01(characterController.velocity.magnitude / maxWalkingSpeed);
					animator.SetFloat("WalkingSpeed", normalizedSpeed);
				} else {
					animator.SetFloat("WalkingSpeed", 0f);
				}
				break;
			case ActionState.Aiming:
				animator.SetBool("IsAiming", active);
				if (active) {
					CameraFOVController.Instance.SetFOV(50, 0.3f);
				} else {
					CameraFOVController.Instance.SetFOV(60, 0.2f);
				}
				break;
		}
	}

	public void SetAnimationOverrideController(AnimatorOverrideController overrideController) {
		if (overrideController != null) {
			animator.runtimeAnimatorController = overrideController;
		} else {
			animator.runtimeAnimatorController = defaultRuntimeAnimatorController;
		}
	}
}
