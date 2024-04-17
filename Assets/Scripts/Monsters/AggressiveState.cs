using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;  // Required for NavMeshAgent

public class AggressiveState : MonsterState
{
    private NavMeshAgent agent;  // Reference to the NavMeshAgent component
    private Transform playerTransform;
    private float aggressiveTimer;  // Timer to track aggression duration
    Animator animator;

    public AggressiveState(GameObject monster, MonsterData monsterData) : base(monster, monsterData)
    {
        agent = monster.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing from the monster!");
        }
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        navMeshAgent.speed = monsterData.runSpeed;
        animator = monster.GetComponentInChildren<Animator>();
        animator.SetBool("IsRunning", true);
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsIdle", false);
        animator.Play("Run");
    }

    public override void Enter()
    {
        base.Enter();
        //Debug.Log("Monster becomes aggressive!");

        aggressiveTimer = 0;  // Reset the aggression timer

        if (agent != null)
        {
            agent.speed = monsterData.runSpeed;  // Set the chasing speed
            agent.angularSpeed = monsterData.turnSpeed;  // Set how quickly the monster can turn
        }
    }

    public override void Execute()
    {
        if (agent == null) return;

        // Update the timer each frame
        aggressiveTimer += Time.deltaTime;

        // Set the destination of the NavMeshAgent to the player's position continuously
        agent.SetDestination(playerTransform.position);

        // Check if the conditions to stop being aggressive are met
        if (Vector3.Distance(monster.transform.position, playerTransform.position) > monsterData.detectionRadius && aggressiveTimer > monsterData.minimumAggressionTime)
        {
            // If the player is outside the detection radius and the minimum time has elapsed
            monster.GetComponent<MonsterController>().ChangeState(new ExploringState(monster, monsterData, monster.GetComponent<MonsterController>().explorationTarget));
        }
    }

    public override void Exit()
    {
        base.Exit();
        //Debug.Log("Monster stops being aggressive.");

        if (agent.isActiveAndEnabled)
        {
            agent.ResetPath();
        }
    }
}
