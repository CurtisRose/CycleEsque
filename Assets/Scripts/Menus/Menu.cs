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

    //TODO: Maybe add some sort of priority system.... I don't know
    // When you are running around with the inventory open, I don't want the
    // Pick up menu to turn it off. That's dumb.
    // Maybe they can be open at the same time. Like pickup window never turns off other windows?
    // Or the pick up window can't be open at the same time as inventory using priority for random menus (specifically, NOT menus intentionally opened by user.)

    void Start()
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
