using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    [SerializeField] private List<Menu> menus = new List<Menu>();

    [SerializeField] private List<Menu> menuStack = new List<Menu>();

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
            if (menuStack.Count > 0)
            {
                Menu menuToClose = menuStack[menuStack.Count - 1];
                menuToClose.Close();
                menuStack.RemoveAt(menuStack.Count - 1);
            }
            if (menuStack.Count > 0)
            {
                Menu nextMenu = menuStack[menuStack.Count - 1];
                nextMenu.Open();
            }
        }
    }

    public void RegisterMenu(Menu menu)
    {
        if (!menus.Contains(menu))
        {
            menus.Add(menu);
        }
    }

    public void OpenMenu(Menu menuToOpen)
    {
        // Don't open if already open
        if (menuStack.Contains(menuToOpen))
        {
            return;
        }

        if (menuStack.Count > 0)
        {
            menuStack[menuStack.Count - 1].Close();
        }
        menuStack.Add(menuToOpen);
        menuToOpen.Open();
    }

    public void CloseMenu(Menu menu)
    {
        if (menuStack.Count > 0)
        {
            int index = -1;
            for (int i = 0; i < menuStack.Count; i++)
            {
                if (menu == menuStack[i])
                {
                    index = i;
                    break;
                }
            }
            if (index >= 0)
            {
                menuStack[index].Close();
                menuStack.RemoveAt(index);
                if (menuStack.Count > 0)
                {
                    menuStack[menuStack.Count - 1].Open();
                }
            }
        }
    }
}
