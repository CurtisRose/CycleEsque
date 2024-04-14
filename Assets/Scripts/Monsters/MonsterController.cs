using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    private MonsterState currentState;
    [SerializeField] MonsterData monsterData;
    [SerializeField] private Health healthComponent;
    private Collider[] hitColliders;
    [SerializeField] public Transform explorationTarget;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] NavMeshPathVisualizer visualizer;

    void Start()
    {
        ChangeState(new ExploringState(gameObject, monsterData, explorationTarget));
        healthComponent.OnHealthChanged += HandleHealthChanged;
        healthComponent.OnDeath += HandleDeath;
    }

    void Update()
    {
        if (currentState != null)
            currentState.Execute();

        // Proximity check to switch to aggressive state
        hitColliders = Physics.OverlapSphere(transform.position, monsterData.detectionRadius);
        bool playerNearby = false;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.CompareTag("Player"))
            {
                playerNearby = true;
                break;
            }
        }

        if (playerNearby && !(currentState is AggressiveState))
        {
            ChangeState(new AggressiveState(gameObject, monsterData));
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
            ChangeState(new FleeingState(gameObject, monsterData));
        }
    }

    private void HandleDeath()
    {
        Destroy(this);
        Destroy(healthComponent);
        Destroy(visualizer);
        Destroy(agent,.01f); // Because of dumb thing about visualizer needing it...
        agent.isStopped = true;
    }
}