using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour, IMenu
{
    [SerializeField] GameObject menuPanel;
    bool isOpen = false;
    [SerializeField] bool affectsUserInput;

    void Awake()
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
        Character.SetUserInputStatus(true);
    }

    public void Open()
    {
        menuPanel.SetActive(true);
        isOpen = true;
        if (affectsUserInput)
        {
            Character.SetUserInputStatus(false);
        }
    }
}
