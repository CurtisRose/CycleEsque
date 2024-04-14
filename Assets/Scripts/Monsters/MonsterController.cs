using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    private MonsterState currentState;
    [SerializeField] MonsterData monsterData;
    [SerializeField] private Health healthComponent;
    private Collider[] hitColliders;
    [SerializeField] public Transform explorationTarget;

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
        this.enabled = false;
        healthComponent.enabled = false;
    }
}