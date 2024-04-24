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

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Tab)) {

			if (PlayerInventoryMenu.Instance != null) {
				if (!IsOpen()) {
					MenuManager.Instance.OpenMenu(PlayerInventoryMenu.Instance);
				} else {
					MenuManager.Instance.CloseMenu(PlayerInventoryMenu.Instance);
				}
			}
		}
	}
}
