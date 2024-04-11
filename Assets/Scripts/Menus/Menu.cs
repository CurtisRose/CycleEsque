using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour, IMenu
{
    [SerializeField] GameObject menuPanel;
    bool isOpen = false;
    [SerializeField] bool affectsUserMovement;
    [SerializeField] bool affectsUserClicking;
    [SerializeField] bool affectsUserLooking;

    void Awake()
    {
        MenuManager.Instance.RegisterMenu(this);
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public void Close()
    {
        menuPanel.SetActive(false);
        isOpen = false;
        if (affectsUserMovement)
        {
            Character.SetUserMovementInputStatus(true);
        }
        if (affectsUserClicking)
        {
            Character.SetUserClickingInputStatus(true);
        }
        if (affectsUserLooking)
        {
            Character.SetUserLookingInputStatus(true);
        }
    }

    public void Open()
    {
        menuPanel.SetActive(true);
        isOpen = true;
        if (affectsUserMovement)
        {
            Character.SetUserMovementInputStatus(false);
        }
        if (affectsUserClicking)
        {
            Character.SetUserClickingInputStatus(false);
        }
        if (affectsUserLooking)
        {
            Character.SetUserLookingInputStatus(false);
        }
    }
}
