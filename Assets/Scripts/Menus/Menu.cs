using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuType
{
    System,      // System-level menus that block all other interactions
    Inventory,      // Main game UIs, such as inventory, that can overlap with other MainUIs
    Interaction  // Interaction menus that require immediate attention but are lower than MainUI
}

public class Menu : MonoBehaviour, IMenu
{
    [SerializeField] protected  GameObject menuPanel;
    [SerializeField] bool isOpen = false;

    [SerializeField] MenuType type;
    [SerializeField] int priority = 0; // Lower number = lower priority

    [SerializeField] bool affectsUserMovement;
    [SerializeField] bool affectsUserClicking;
    [SerializeField] bool affectsUserLooking;

    public List<Menu> dependentMenus;

    //TODO: Maybe add some sort of priority system.... I don't know
    // When you are running around with the inventory open, I don't want the
    // Pick up menu to turn it off. That's dumb.
    // Maybe they can be open at the same time. Like pickup window never turns off other windows?
    // Or the pick up window can't be open at the same time as inventory using priority for random menus (specifically, NOT menus intentionally opened by user.)

    void Start()
    {
        
    }

    public int Priority => priority; // Property to expose the priority

    public MenuType GetMenuType()
    {
        return type;
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public virtual void Close()
    {
        menuPanel.SetActive(false);
        isOpen = false;
        if (affectsUserMovement)
        {
            Player.SetUserMovementInputStatus(true);
        }
        if (affectsUserClicking)
        {
            Player.SetUserClickingInputStatus(true);
        }
        if (affectsUserLooking)
        {
            Player.SetUserLookingInputStatus(true);
        }
    }

    public virtual void Open()
    {
        menuPanel.SetActive(true);
        isOpen = true;
        OpenDependentMenus();
        if (affectsUserMovement)
        {
            Player.SetUserMovementInputStatus(false);
        }
        if (affectsUserClicking)
        {
            Player.SetUserClickingInputStatus(false);
        }
        if (affectsUserLooking)
        {
            Player.SetUserLookingInputStatus(false);
        }
    }

    private void OpenDependentMenus()
    {
        foreach (Menu menu in dependentMenus)
        {
            if (!menu.IsOpen())
            {
                MenuManager.Instance.OpenMenu(menu);
            }
        }
    }
}
