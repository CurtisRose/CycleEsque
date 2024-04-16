using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MonsterPart : MonoBehaviour
{
    private MonsterController parentMonster; // Reference to the parent monster script
    private Rigidbody rigidBody;

    private void Start()
    {
        // Find the parent monster script in the hierarchy
        parentMonster = GetComponentInParent<MonsterController>();
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.isKinematic = true;
        rigidBody.useGravity = false;
    }

    public void HandleHit(Projectile projectile)
    {
        if (parentMonster != null)
        {
            parentMonster.HandleHit(projectile);
        } else
        {
            // If the parentMonster doesn't exist, it's probably dead.
            Destroy(this);
        }
    }
}