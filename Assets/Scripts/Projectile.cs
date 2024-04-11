    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] float lifespan;
    [SerializeField] float speed;
    [SerializeField] float damage;
    [SerializeField] float armorPenetration;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
        Destroy(this.gameObject, lifespan);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageableObject))
        {
            damageableObject.TakeDamage(damage, armorPenetration);
        }

        Destroy(gameObject);
    }
}
