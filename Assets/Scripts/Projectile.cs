using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float lifespan = 5f;
    [SerializeField] private float speed = 100f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float armorPenetration = 0.5f;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.velocity = transform.forward * speed;
        Invoke("ReturnToPool", lifespan);Debug.DrawRay(transform.position, transform.forward * 10, Color.blue, 2.0f);
    }   

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageableObject))
        {
            damageableObject.TakeDamage(damage, armorPenetration);
        }


        PlayImpactEffect();

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        ProjectilePool.Instance.ReturnProjectile(gameObject);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void PlayImpactEffect()
    {
        
    }
}