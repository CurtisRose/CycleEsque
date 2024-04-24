using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBoxMenu : Menu
{
    private void Awake()
    {
        dependentMenus = new List<Menu>();
    }

    void Start() {
        // Let all the inventory slots do what they need to do
        Open();
        Close();
        dependentMenus.Add(PlayerInventoryMenu.Instance);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsOpen())
            {
                MenuManager.Instance.CloseMenu(this);
            }
        }
    }
}
