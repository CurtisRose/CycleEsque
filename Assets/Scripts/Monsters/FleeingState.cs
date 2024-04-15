using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;  // Required for NavMeshAgent

public class FleeingState : MonsterState
{
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Vector3 destination;

    public FleeingState(GameObject monster, MonsterData monsterData) : base(monster, monsterData)
    {
        agent = monster.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing from the monster!");
        }
    }

    public override void Enter()
    {
        base.Enter();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        PickRandomFleeDirection();
    }

    public override void Execute()
    {
        if (agent == null) return;

        // Check if monster has reached near the destination or not
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (Vector3.Distance(monster.transform.position, playerTransform.position) > monsterData.detectionRadius)
            {
                // If far enough from the player, switch to exploring state
                monster.GetComponent<MonsterController>().ChangeState(new ExploringState(monster, monsterData, monster.GetComponent<MonsterController>().explorationTarget));
            }
            else
            {
                // Otherwise, pick a new fleeing direction
                PickRandomFleeDirection();
            }
        }
    }

    private void PickRandomFleeDirection()
    {
        // Generate a random direction to flee towards
        Vector3 randomDirection = Random.insideUnitSphere * monsterData.fleeDistance + monster.transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, monsterData.fleeDistance, NavMesh.AllAreas))
        {
            destination = hit.position;
            agent.SetDestination(destination);
        }
        else
        {
            Debug.Log("No valid navmesh point found in the flee direction!");
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (agent.isActiveAndEnabled)
        {
            agent.ResetPath();
        }
    }
}
