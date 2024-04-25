using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;  // Required for NavMeshAgent

public class AggressiveState : MonsterState
{
    private NavMeshAgent agent;  // Reference to the NavMeshAgent component
    private Player player;
    private float aggressiveTimer;  // Timer to track aggression duration
    Animator animator;

    public AggressiveState(GameObject monster, MonsterData monsterData) : base(monster, monsterData)
    {
        agent = monster.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing from the monster!");
        }
        player = monster.GetComponent<MonsterController>().GetPlayers()[0];
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
        monster.GetComponent<MonsterController>().SetTarget(player.transform);
    }

    public override void Execute()
    {
        if (agent == null) return;

        // Update the timer each frame
        aggressiveTimer += Time.deltaTime;

        // Continuously set the destination to the player's position
        agent.SetDestination(player.transform.position);

        // Check for attack range
        if (Vector3.Distance(monster.transform.position, player.transform.position) <= monsterData.attackRange)
        {
            // If within attack range, switch to attack state
            monster.GetComponent<MonsterController>().ChangeState(new AttackState(monster, monsterData));
            return;  // Ensure no further execution in this state after switching
        }

        // Check if the conditions to stop being aggressive are met
        if (Vector3.Distance(monster.transform.position, player.transform.position) > monsterData.detectionRadius && aggressiveTimer > monsterData.minimumAggressionTime)
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
