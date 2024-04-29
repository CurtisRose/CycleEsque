using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Flags]
public enum ActionState
{
    Idle = 0,
    Shooting = 1 << 0,
    Reloading = 1 << 1,
    SwappingWeapons = 1 << 2,
    Aiming = 1 << 3,
    UsingConsumable = 1 << 4
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
            DontDestroyOnLoad(this.gameObject); // Optional: Makes it persist across scenes
        }
    }

    public void EnterState(ActionState newState)
    {
        currentState |= newState; // Adds the new state to the current state
    }

    public void ExitState(ActionState state)
    {
        currentState &= ~state; // Removes the state from the current state
    }

    public bool IsInState(ActionState state)
    {
        return (currentState & state) == state;
    }

    public bool CanPerformAction(ActionState actionState)
    {
        switch (actionState)
        {
            case ActionState.Shooting:
                // Shooting can occur only if not reloading or swapping weapons
                return !IsInState(ActionState.Reloading) && !IsInState(ActionState.SwappingWeapons) && !IsInState(ActionState.UsingConsumable);
            case ActionState.Reloading:
                return !IsInState(ActionState.Reloading) && !IsInState(ActionState.Shooting) && !IsInState(ActionState.SwappingWeapons) && !IsInState(ActionState.UsingConsumable);
            case ActionState.SwappingWeapons:
                // Reloading and swapping weapons cannot happen while shooting or aiming
                return !IsInState(ActionState.Shooting) && !IsInState(ActionState.SwappingWeapons) && !IsInState(ActionState.UsingConsumable);
            case ActionState.Aiming:
                // Aiming cannot occur while reloading or swapping
                return !IsInState(ActionState.Reloading) && !IsInState(ActionState.SwappingWeapons) && !IsInState(ActionState.UsingConsumable);
            case ActionState.UsingConsumable:
				// Using a consumable can only occur during idle state
                return IsInState(ActionState.Idle);
            default:
                return true;
        }
    }
}
