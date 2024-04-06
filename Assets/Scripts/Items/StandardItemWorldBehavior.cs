using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardItemWorldBehavior : MonoBehaviour
{
    [SerializeField] private Transform body;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float bobSpeed;
    [SerializeField] private float range;

    private void Awake()
    {
        bobSpeed += Random.Range(-0.1f, 0.1f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        body.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        Vector3 temp = body.localPosition;
        temp.y = Mathf.Sin(Time.fixedTime * bobSpeed) * range;
        body.localPosition = temp;
    }
}
