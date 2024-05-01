using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MonsterController : MonoBehaviour
{
    [SerializeField] MonsterData monsterData;
    [SerializeField] public Transform explorationTarget;
    [SerializeField] Transform rootmotionObject;

    private MonsterState currentState;
    private Health healthComponent;
    private MonsterHealthUIController healthUIController;
    [SerializeField] private Transform healthbarCanvas;
    private Collider[] hitColliders;
    private NavMeshAgent agent;
    private NavMeshPathVisualizer visualizer;

    List<Player> players = new List<Player>();
    LayerMask layerMask;

    Animator animator;

    [SerializeField] bool DrawGizmos;

    public delegate void Death();
    public event Death OnDeath;

    private Transform targetTransform;
    [SerializeField] private LootPool lootPool;
    private ItemDropper itemDropper;

    bool isDead = false;
    public WorldItem itemDropped;

    AudioSource audioSource;
	[SerializeField] List<AudioClip> painFleeingSounds;
	SoundRandomizer painFleeingRandomClips;

    [SerializeField] string state;

	private void Awake()
    {
		audioSource = GetComponent<AudioSource>();
		painFleeingRandomClips = new SoundRandomizer(painFleeingSounds);
		healthComponent = GetComponent<Health>();
        healthComponent.SetMaxHealth(monsterData.health);
        healthUIController = GetComponent<MonsterHealthUIController>();
        InitializeAgent();
        visualizer = GetComponent<NavMeshPathVisualizer>();
        animator = GetComponentInChildren<Animator>();
        layerMask = 1 << LayerMask.NameToLayer("Player");
        itemDropper = GetComponent<ItemDropper>();
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
        if (isDead)
			return;
        if (currentState != null)
            currentState.Execute();
    }

	public void Destroy() {
		Destroy(gameObject);
		if (itemDropped != null) {
			Destroy(itemDropped.gameObject);
		}
	}

	void FetchPlayers()
    {
        // Find all game objects tagged as "Player" and add their Character component to the list
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObject in playerObjects)
        {
			Player player = playerObject.GetComponent<Player>();
            if (player != null)
            {
                players.Add(player);
            }
        }
    }

    public void ChangeState(MonsterState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;

        if (newState is ExploringState) {
            state = "ExplorationState";
        } else if (newState is AggressiveState) {
			state = "AggressiveState";
		} else if (newState is AttackState) {
			state = "AttackState";
		} else if (newState is FleeingState) {
			state = "FleeingState";
		} else {
			state = "UnknownState";
		}

		if (newState as FleeingState != null) {
            MakeSound(painFleeingRandomClips.GetRandomClip());
        }

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
        if (!audioSource.isPlaying) {
			MakeSound(painFleeingRandomClips.GetRandomClip());
		}

		isDead = true;
		OnDeath?.Invoke();
		healthComponent.OnHealthChanged -= HandleHealthChanged;
        healthComponent.OnDeath -= HandleDeath;
        Destroy(visualizer);
        Destroy(agent,.01f); // Because of dumb thing about visualizer needing it...
		Destroy(healthComponent);
		Destroy(healthUIController, healthComponent.GetVisibilityTimeAfterDeath());
		Destroy(healthbarCanvas.gameObject, healthComponent.GetVisibilityTimeAfterDeath());

		agent.isStopped = true;
        animator.Play("Death");
        WorldItem item = lootPool.GetRandomItemWithQuantity();
        if (item != null) {
			itemDropped = itemDropper.DropItem(item.CreateItemInstance());
		}
        Destroy(this);
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

        // If the target is too far away, don't apply damage
        // Using the effective attack range here
        if (Vector3.Distance(transform.position, targetTransform.position) > monsterData.effectiveAttackRange)
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

    public List<Player> GetPlayers()
    {
        return players;
    }

	public void MakeSound(AudioClip audioClip) {
		audioSource.pitch = Random.Range(0.95f, 1.05f); // Adjust pitch slightly
		audioSource.volume = Random.Range(0.8f, 1.0f); // Adjust volume slightly
		audioSource.PlayOneShot(audioClip);
	}

	public void HearNoise(Vector3 noiseSource, PlayerNoiseLevel noiseLevel) {
		if (currentState is AggressiveState || currentState is AttackState) {
			// If already in one of these states, just return
			return;
		}
		float distanceToPlayer = Vector3.Distance(transform.position, noiseSource);
		Debug.Log($"Heard noise at level {noiseLevel} from distance {distanceToPlayer}m");

		//AlertOtherMonsters();

		ChangeState(new AggressiveState(gameObject, monsterData));
	}

	private void AlertOtherMonsters() {
		// Emit sound at High level
		if (!audioSource.isPlaying) {
			MakeSound(painFleeingRandomClips.GetRandomClip());
		}

		PlayerSoundController.Instance.RegisterSound(PlayerNoiseLevel.High, transform.position, true);
	}
}