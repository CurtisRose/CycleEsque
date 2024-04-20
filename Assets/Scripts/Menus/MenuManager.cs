using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    [SerializeField] private List<Menu> menus = new List<Menu>();  // Track all registered menus
    private List<Menu> menuStack = new List<Menu>();  // Stack for open menus

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTopMenu();
        }
    }

    // Register a menu to the manager
    public void RegisterMenu(Menu menu)
    {
        if (!menus.Contains(menu))
        {
            menus.Add(menu);
        }
    }

    public void OpenMenu(Menu menuToOpen)
    {
        // Ensure the menu is registered if it's not already in the menus list
        if (!menus.Contains(menuToOpen))
        {
            RegisterMenu(menuToOpen);
        }

        // Check if the menu is already in the stack; if so, do nothing further
        if (menuStack.Contains(menuToOpen))
        {
            return;
        }

        // Close the current top menu if the new menu has a higher or equal priority
        if (menuStack.Count > 0)
        {
            Menu topMenu = menuStack[menuStack.Count - 1];
            if (menuToOpen.Priority >= topMenu.Priority)
            {
                topMenu.Close();
            }
        }

        // Add the new menu to the stack and open it
        menuStack.Add(menuToOpen);
        menuToOpen.Open();
    }

    public void CloseTopMenu()
    {
        if (menuStack.Count > 0)
        {
            Menu menuToClose = menuStack[menuStack.Count - 1];
            menuToClose.Close();
            menuStack.RemoveAt(menuStack.Count - 1);

            if (menuStack.Count > 0)
            {
                Menu nextMenu = menuStack[menuStack.Count - 1];
                nextMenu.Open();
            }
        }
    }

    public void CloseMenu(Menu menu)
    {
        int index = menuStack.IndexOf(menu);
        if (index != -1)
        {
            menu.Close();
            menuStack.RemoveAt(index);

            if (index == menuStack.Count && menuStack.Count > 0)
            {
                Menu nextMenu = menuStack[menuStack.Count - 1];
                nextMenu.Open();
            }
        }
    }
}