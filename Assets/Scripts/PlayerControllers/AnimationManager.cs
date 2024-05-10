using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationCommand {
	IdleHip, // Idle Gun at Hip
	WalkHip, // Gun at hip
	IdleAim,
	WalkAim,
	Run, // Gun always at him when running, can't aim while running
	Jump, // Gun always at hip when jumping, can't aim while jumping
	Shoot,
	Reload
}

public class AnimationManager : MonoBehaviour
{
	public static AnimationManager Instance;
	[SerializeField] private Animator animator;

	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
	}

	public void HandleAnimationCommand(AnimationCommand command) {
		switch (command) {
			case AnimationCommand.IdleHip:
				Debug.Log("Doing Idle Animation");
				animator.ResetTrigger("Aim");
				animator.SetTrigger("Idle");
				CameraFOVController.Instance.SetFOV(60, 0.2f);
				break;
			case AnimationCommand.IdleAim:
				Debug.Log("Doing Aiming Animation");
				animator.ResetTrigger("Idle");
				animator.SetTrigger("Aim");
				CameraFOVController.Instance.SetFOV(50, 0.3f);
				break;
			case AnimationCommand.WalkHip:
				Debug.Log("Doing Walking Animation");
				animator.SetTrigger("Walk");
				break;
		}
	}
}
