using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    [SerializeField] private List<Menu> activeMenus = new List<Menu>();
    [SerializeField] bool DontDestroyOnLoadToggle;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (DontDestroyOnLoadToggle)
            {
                DontDestroyOnLoad(gameObject);
            }   
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

    // Close dependents is used in the case of the player inventory and the loot box menus.
    // Opening the loot box opens up the loot boxes dependent menu, the play inventory menu.
    // When you close the lootbox with tab, it would typically try to also close it's dependents
    // The problem is, the player inventory menu is also listening to tab, and will get executed after and reopen itself.
    // In this case, I want the loot box menu to open the player inventory menu, but not to close it.
    // Let the player inventory manage itself.
    // TODO: Consider centralizing menu inputs to this class. So this class handles the logic of pressing tab based on conditions of what menus are open or not.
    public void CloseMenu(Menu menu, bool closeDependents = true)
    {
        if (activeMenus.Contains(menu))
        {
            menu.Close();
            activeMenus.Remove(menu);
            if (closeDependents) {
                foreach (var dependentMenu in menu.dependentMenus) {
                    CloseMenu(dependentMenu); // Optionally close dependent menus
                }
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