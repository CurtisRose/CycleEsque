using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryMenu : Menu
{
    public static PlayerInventoryMenu Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

}
