using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WorldItem : MonoBehaviour
{
    [SerializeField] BaseItem item;
    Rigidbody rigidBody;

    [SerializeField] float timeDelay = 1.0f;
    bool interactable = true;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {
        StartCoroutine(TempInteractionOff(timeDelay));
    }

    public BaseItem GetBaseItem()
    {
        return item;
    }

    public void Equip()
    {
        if (rigidBody != null)
        {
            rigidBody.isKinematic = true;
        }
        interactable = false;
    }

    IEnumerator TempInteractionOff(float delay)
    {
        interactable = false;
        yield return new WaitForSeconds(delay);
        interactable = true;
    }

    public bool IsInteractable()
    {
        return interactable;
    }
}
