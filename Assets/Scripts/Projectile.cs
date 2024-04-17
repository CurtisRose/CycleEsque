using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float lifespan = 5f; // This can remain fixed or be adjusted by the Gun class as well
    [SerializeField] private GameObject visualProjectile;
    [SerializeField] private float alignmentDuration = 0.1f;
    [SerializeField] private float raycastBackOffset = 1.0f;
    private float alignmentTimer;

    public float Speed { get; set; }
    public float Damage { get; set; }
    public float ArmorPenetration { get; set; }

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.velocity = transform.forward * Speed;
        Invoke("ReturnToPool", lifespan);
        alignmentTimer = alignmentDuration; // Initialize alignment timer
        Debug.DrawRay(transform.position, transform.forward * 10, Color.blue, 2.0f);

        // Check if the projectile is spawned inside a monster
        CheckInitialRaycast();
    }

    private void CheckInitialRaycast()
    {
        Vector3 startRaycastPoint = transform.position - transform.forward * raycastBackOffset; // Start the raycast from a point behind the projectile
        RaycastHit hit;
        float raycastLength = raycastBackOffset + 0.5f; // Length of the raycast; includes the backward offset and a small forward distance

        if (Physics.Raycast(startRaycastPoint, transform.forward, out hit, raycastLength, LayerMask.GetMask("Monster")))
        {
            Debug.DrawRay(startRaycastPoint, transform.forward * raycastLength, Color.red, 60.0f); // Draw raycast in red if it hits something
            if (hit.collider.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.ReceiveDamage(Damage);
                Debug.Log("Projectile spawned and hit a monster immediately!");
                PlayImpactEffect();
                ReturnToPool(); // Immediately return to pool after delivering damage
            }
        }
        else
        {
            Debug.DrawRay(startRaycastPoint, transform.forward * raycastLength, Color.green, 60.0f); // Draw raycast in green if it hits nothing
        }
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
        if (collision.gameObject.TryGetComponent<MonsterPart>(out MonsterPart monsterPart))
        {
            monsterPart.HandleHit(this);
        } else if (collision.gameObject.TryGetComponent<MonsterController>(out MonsterController _))
        {
            // don't destroy the bullet, it's hitting the agent collider.
            return;
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