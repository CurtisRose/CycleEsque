using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Flags]
public enum ActionState
{
    Idle = 0,
    Shooting = 1 << 0,
    Reloading = 1 << 1,
    Swapping = 1 << 2,
    Aiming = 1 << 3,
    UsingConsumable = 1 << 4,
    Walking = 1 << 5,
    Running = 1 << 6
}

public class ActionStateManager : MonoBehaviour
{
    public static ActionStateManager Instance { get; private set; }

    [SerializeField] private ActionState currentState = ActionState.Idle;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void EnterState(ActionState newState)
    {
        currentState |= newState; // Adds the new state to the current state
        if (newState == ActionState.Aiming) {
			AnimationManager.Instance.HandleAnimationCommand(AnimationCommand.IdleAim);
		}
    }

    public void ExitState(ActionState state)
    {
        currentState &= ~state; // Removes the state from the current state
		if (state == ActionState.Aiming) {
			AnimationManager.Instance.HandleAnimationCommand(AnimationCommand.IdleHip);
		}
	}

    public bool IsInState(ActionState state)
    {
        return (currentState & state) == state;
    }

	public bool CanPerformAction(ActionState actionState) {
		// Check if any action that blocks other actions is active
		bool isBusy = IsInState(ActionState.Reloading | ActionState.Swapping | ActionState.UsingConsumable);
		switch (actionState) {
			case ActionState.Shooting:
				return !isBusy && !IsInState(ActionState.Shooting);
			case ActionState.Reloading:
			case ActionState.Swapping:
			case ActionState.UsingConsumable:
				return !isBusy;
			case ActionState.Aiming:
				return !isBusy;
			default:
				return true;
		}
	}
}
