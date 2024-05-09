using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationCommand {
	Idle,
	Run,
	Jump,
	Shoot,
	Reload,
	Crouch, 
	Aim
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
			case AnimationCommand.Idle:
				Debug.Log("Doing Idle Animation");
				animator.SetTrigger("Idle");
				break;
			case AnimationCommand.Aim:
				Debug.Log("Doing Aiming Animation");
				animator.SetTrigger("Aim");
				break;
		}
	}
}
