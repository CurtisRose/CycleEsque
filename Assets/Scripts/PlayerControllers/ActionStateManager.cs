using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionStateManager : MonoBehaviour {
	public static ActionStateManager Instance { get; private set; }

	public bool IsAiming { get; private set; }
	public bool IsWalking { get; private set; }
	public bool IsRunning { get; private set; }
	public bool IsReloading { get; private set; }
	public bool IsShooting { get; private set; }
	public bool IsUsingConsumable { get; private set; }
	public bool IsSwapping { get; private set; }

	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(this.gameObject);
		} else {
			Instance = this;
		}
	}

	// Methods to attempt to set states
	public bool TrySetAiming(bool aiming) {
		if (aiming) {
			if (CanPerformAction(ActionState.Aiming)) {
				IsAiming = true;
				AnimationManager.Instance.HandleAnimationCommand(ActionState.Aiming, true);
				return true;
			}
			return false;
		}
		IsAiming = false;
		AnimationManager.Instance.HandleAnimationCommand(ActionState.Aiming, false);
		return true;
	}

	public bool TrySetWalking(bool walking) {
		IsWalking = walking;
		AnimationManager.Instance.HandleAnimationCommand(ActionState.Walking, walking);
		return true;
	}

	public bool TrySetRunning(bool running) {
		IsRunning = running;
		AnimationManager.Instance.HandleAnimationCommand(ActionState.Running, running);
		if (running) {
			// If you begin running, undo aiming if aiming.
			if(IsAiming) {
				TrySetAiming(false);
			}
		}
		return true;
	}

	public bool TrySetReloading(bool reloading) {
		if (reloading) {
			if (CanPerformAction(ActionState.Reloading)) {
				IsReloading = true;
				return true;
			}
			return false;
		}
		IsReloading = false;
		return true;
	}

	public bool TrySetShooting(bool shooting) {
		if (shooting) {
			if (CanPerformAction(ActionState.Firing)) {
				IsShooting = true;
				AnimationManager.Instance.HandleAnimationCommand(ActionState.Firing, shooting);
				return true;
			}
			return false;
		}
		IsShooting = false;
		return true;
	}

	public bool TrySetUsingConsumable(bool usingConsumable) {
		if (usingConsumable) {
			if (CanPerformAction(ActionState.UseConsumable)) {
				IsUsingConsumable = true;
				return true;
			}
			return false;
		}
		IsUsingConsumable = false;
		return true;
	}
	public bool TrySetSwapping(bool swapping) {
		if (swapping) {
			if (CanPerformAction(ActionState.Swapping)) {
				IsSwapping = true;
				return true;
			}
			return false;
		}
		IsSwapping = false;
		return true;
	}

	// Check if an action can be performed
	public bool CanPerformAction(ActionState actionState) {
		switch (actionState) {
			case ActionState.Reloading:
			case ActionState.UseConsumable:
			case ActionState.Swapping:
				return !IsShooting && !IsReloading && !IsUsingConsumable && !IsSwapping;
			case ActionState.Aiming:
				return !IsReloading && !IsUsingConsumable && !IsSwapping && !IsRunning;
			case ActionState.Firing:
				return !IsShooting && !IsReloading && !IsUsingConsumable && !IsSwapping && !IsRunning;
			default:
				return true;
		}
	}
}

public enum ActionState {
	Aiming,
	Walking,
	Running,
	Reloading,
	Firing,
	UseConsumable,
	Swapping
}
