using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float lifespan = 5f;
    [SerializeField] private float speed = 100f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float armorPenetration = 0.5f;
    [SerializeField] private GameObject visualProjectile; // The visual component of the projectile
    [SerializeField] private float alignmentDuration = 0.1f; // Duration over which the visual aligns with the logical path
    private float alignmentTimer;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.velocity = transform.forward * speed;
        Invoke("ReturnToPool", lifespan);
        Debug.DrawRay(transform.position, transform.forward * 10, Color.blue, 2.0f);
    }

    private void Update()
    {
        if (alignmentTimer > 0)
        {
            float alignProgress = 1f - (alignmentTimer / alignmentDuration);
            visualProjectile.transform.localPosition = Vector3.Lerp(new Vector3(0, -0.5f, 0), Vector3.zero, alignProgress);
            alignmentTimer -= Time.deltaTime;
        }
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
        ProjectilePool.Instance.ReturnProjectile(this);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void PlayImpactEffect()
    {
        // Impact effect logic goes here
    }

    public void SetInitialVisualPosition(float offset)
    {
        if (visualProjectile != null)
        {
            visualProjectile.transform.localPosition = new Vector3(0, offset, 0);
            alignmentTimer = alignmentDuration; // Reset the alignment timer
        }
    }
}