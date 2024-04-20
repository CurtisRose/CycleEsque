using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopCanvas : MonoBehaviour
{
    // Static singleton property
    public static TopCanvas Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Makes the instance persistent between scenes
        }
    }

    public void AddElementToCanvas(Transform element)
    {
        element.SetParent(transform, true); // Set as a child of this canvas, maintaining world position
    }

    public void RemoveElementFromCanvas(Transform element, Transform originalParent)
    {
        element.SetParent(originalParent, true); // Return to the original parent, maintaining world position
    }
}
