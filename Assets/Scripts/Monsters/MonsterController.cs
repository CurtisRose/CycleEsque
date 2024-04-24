using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    [SerializeField] MonsterData monsterData;
    [SerializeField] public Transform explorationTarget;
    [SerializeField] Transform rootmotionObject;

    private MonsterState currentState;
    private Health healthComponent;
    private MonsterHealthUIController healthUIController;
    private Collider[] hitColliders;
    private NavMeshAgent agent;
    private NavMeshPathVisualizer visualizer;

    List<Character> players = new List<Character>();
    LayerMask layerMask;

    Animator animator;

    [SerializeField] bool DrawGizmos;

    public delegate void Death();
    public event Death OnDeath;

    private Transform targetTransform;

    private void Awake()
    {
        healthComponent = GetComponent<Health>();
        healthComponent.SetMaxHealth(monsterData.health);
        healthUIController = GetComponent<MonsterHealthUIController>();
        InitializeAgent();
        visualizer = GetComponent<NavMeshPathVisualizer>();
        animator = GetComponentInChildren<Animator>();
        layerMask = 1 << LayerMask.NameToLayer("Player");
    }

    void Start()
    {
        ChangeState(new ExploringState(gameObject, monsterData, explorationTarget));
        healthComponent.OnHealthChanged += HandleHealthChanged;
        healthComponent.OnDeath += HandleDeath;
        FetchPlayers();
    }

    void InitializeAgent()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = monsterData.walkSpeed;
        agent.angularSpeed = monsterData.turnSpeed;
        agent.acceleration = monsterData.acceleration;
        agent.stoppingDistance = monsterData.stoppingDistance;
    }

    void Update()
    {
        if (currentState != null)
            currentState.Execute();
    }

    void FetchPlayers()
    {
        // Find all game objects tagged as "Player" and add their Character component to the list
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObject in playerObjects)
        {
            Character character = playerObject.GetComponent<Character>();
            if (character != null)
            {
                players.Add(character);
            }
        }
    }

    public void ChangeState(MonsterState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    private void HandleHealthChanged(float currentHealth)
    {
        if (currentHealth < 30f)
        {
            if (monsterData != null)
            {
                ChangeState(new FleeingState(gameObject, monsterData));
            }
        }
    }

    private void HandleDeath()
    {
		OnDeath?.Invoke();
		healthComponent.OnHealthChanged -= HandleHealthChanged;
        healthComponent.OnDeath -= HandleDeath;
        Destroy(this);
        Destroy(healthComponent);
        Destroy(visualizer);
        Destroy(agent,.01f); // Because of dumb thing about visualizer needing it...
        Destroy(healthUIController, healthComponent.GetVisibilityTime());
        agent.isStopped = true;
        animator.Play("Death");
    }

    void OnDrawGizmos()
    {
        if (DrawGizmos)
        {
            // Detection radius
            Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, monsterData.detectionRadius);

        // Flee distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, monsterData.fleeDistance);

            // Exploring radius
            if (explorationTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(explorationTarget.position, monsterData.exploringRadius);
            }
        }
    }

    // This should be done at the end of a root motion movement.
    void AlignParentToRootMotionObject()
    {
        // Set the parent's position and rotation to match the child's
        transform.position = rootmotionObject.position;
        transform.rotation = rootmotionObject.rotation;

        // Reset the child's local position and rotation
        rootmotionObject.localPosition = Vector3.zero;
        rootmotionObject.localRotation = Quaternion.identity;
    }

    public void HandleHit(Projectile projectile, float criticalMultiplier)
    {
        // TODO: Add armor penetration calculations
        healthComponent.ReceiveDamage(projectile.Damage * criticalMultiplier);
        HitMarker.Instance.ShowHitMarker(criticalMultiplier);
    }

    public void ApplyDamage()
    {
        if (targetTransform == null)
            return;

        // Assuming the player has a script component that can receive damage
        IDamageable playerDamageable = targetTransform.GetComponent<IDamageable>();
        if (playerDamageable != null)
        {
            playerDamageable.ReceiveDamage(monsterData.attackDamage);
        }
    }

    public void SetTarget(Transform targetTransform)
    {
        this.targetTransform = targetTransform;
    }

    public List<Character> GetPlayers()
    {
        return players;
    }
}