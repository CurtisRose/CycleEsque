using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;  // Required for NavMeshAgent

public class AggressiveState : MonsterState
{
    private NavMeshAgent agent;  // Reference to the NavMeshAgent component

    public AggressiveState(GameObject monster, MonsterData monsterData) : base(monster, monsterData)
    {
        // Ensure there's a NavMeshAgent component attached to the monster
        agent = monster.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing from the monster!");
        }
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Monster becomes aggressive!");

        // Configure the NavMeshAgent for aggressive behavior
        if (agent != null)
        {
            agent.speed = monsterData.moveSpeed;  // Set the chasing speed
            agent.angularSpeed = monsterData.turnSpeed;  // Set how quickly the monster can turn
        }
    }

    public override void Execute()
    {
        if (agent == null) return;

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Check the distance to the player
        if (Vector3.Distance(monster.transform.position, playerTransform.position) > monsterData.detectionRadius)
        {
            // If the player is outside the detection radius, switch to exploring state
            monster.GetComponent<MonsterController>().ChangeState(new ExploringState(monster, monsterData, monster.GetComponent<MonsterController>().explorationTarget));
        }
        else
        {
            // Set the destination of the NavMeshAgent to the player's position
            agent.SetDestination(playerTransform.position);
        }
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Monster stops being aggressive.");

        // Optionally stop the NavMeshAgent when not aggressive
        if (agent.isActiveAndEnabled)
        {
            agent.ResetPath();
        }
    }
}
