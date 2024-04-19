using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Flags]
public enum WeaponState
{
    Idle = 0,
    Shooting = 1 << 0,
    Reloading = 1 << 1,
    SwappingWeapons = 1 << 2,
    Aiming = 1 << 3
}

public class WeaponStateManager : MonoBehaviour
{
    public static WeaponStateManager Instance { get; private set; }

    [SerializeField] private WeaponState currentState = WeaponState.Idle;

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

    public void EnterState(WeaponState newState)
    {
        currentState |= newState; // Adds the new state to the current state
    }

    public void ExitState(WeaponState state)
    {
        currentState &= ~state; // Removes the state from the current state
    }

    public bool IsInState(WeaponState state)
    {
        return (currentState & state) == state;
    }

    public bool CanPerformAction(WeaponState actionState)
    {
        switch (actionState)
        {
            case WeaponState.Shooting:
                // Shooting can occur only if not reloading or swapping weapons
                return !IsInState(WeaponState.Reloading) && !IsInState(WeaponState.SwappingWeapons);
            case WeaponState.Reloading:
                return !IsInState(WeaponState.Reloading) && !IsInState(WeaponState.Shooting) && !IsInState(WeaponState.SwappingWeapons);
            case WeaponState.SwappingWeapons:
                // Reloading and swapping weapons cannot happen while shooting or aiming
                return !IsInState(WeaponState.Shooting) && !IsInState(WeaponState.SwappingWeapons);
            case WeaponState.Aiming:
                // Aiming cannot occur while reloading or swapping
                return !IsInState(WeaponState.Reloading) && !IsInState(WeaponState.SwappingWeapons);
            default:
                return true;
        }
    }
}
