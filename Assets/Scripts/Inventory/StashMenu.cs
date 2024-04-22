using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StashMenu : Menu
{
     public static StashMenu Instance;

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
