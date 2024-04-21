using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    private List<Menu> activeMenus = new List<Menu>();

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

    public void OpenMenu(Menu menu)
    {
        if (activeMenus.Contains(menu))
        {
            return;  // Menu already open
        }

        // Handle concurrent display logic based on MenuType
        if (menu.GetMenuType() == MenuType.System)
        {
            CloseAll();  // Close all other menus when a System menu is opened
        }
        else if (menu.GetMenuType() == MenuType.Inventory)
        {
            // Allow multiple MainUI menus unless priority conflict
            foreach (var m in activeMenus.FindAll(m => m.GetMenuType() == MenuType.Interaction && m.Priority >= menu.Priority))
            {
                m.Close();
            }
        }

        menu.Open();
        activeMenus.Add(menu);
        SortMenus();
    }

    public void CloseMenu(Menu menu)
    {
        if (activeMenus.Contains(menu))
        {
            menu.Close();
            activeMenus.Remove(menu);
            foreach (var dependentMenu in menu.dependentMenus)
            {
                CloseMenu(dependentMenu); // Optionally close dependent menus
            }
        }
    }

    void CloseTopMenu()
    {
        if (activeMenus.Count > 0)
        {
            var topMenu = activeMenus[activeMenus.Count - 1];
            CloseMenu(topMenu);
        }
    }

    void CloseAll()
    {
        foreach (var menu in activeMenus)
        {
            menu.Close();
        }
        activeMenus.Clear();
    }

    void SortMenus()
    {
        activeMenus.Sort((m1, m2) => m1.Priority.CompareTo(m2.Priority));
    }
}