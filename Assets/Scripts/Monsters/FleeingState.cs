using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;  // Required for NavMeshAgent

public class FleeingState : MonsterState
{
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Vector3 destination;
	private Animator animator;

	public FleeingState(GameObject monster, MonsterData monsterData) : base(monster, monsterData)
    {
		animator = monster.GetComponentInChildren<Animator>();
		agent = monster.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing from the monster!");
        }
        navMeshAgent.speed = monsterData.runSpeed;
    }

    public override void Enter()
    {
        base.Enter();
		animator.SetBool("IsRunning", true);
		playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        PickRandomFleeDirection();
    }

	public override void Execute() {
		if (agent == null) return;

		// Check if monster has reached near the destination or not
		if (!agent.pathPending && agent.remainingDistance < monsterData.stoppingDistance) {
			if (Vector3.Distance(monster.transform.position, playerTransform.position) > monsterData.detectionRadiusInvestigating) {
				// If far enough from the player, switch to exploring state
				monster.GetComponent<MonsterController>().ChangeState(new ExploringState(monster, monsterData, monster.GetComponent<MonsterController>().explorationTarget));
			} else {
				// Otherwise, pick a new fleeing direction
				PickRandomFleeDirection();
			}
		}
	}
	

	private void PickRandomFleeDirection()
    {
		// Direction from the player to the monster
		Vector3 fromPlayer = monster.transform.position - playerTransform.position;
		fromPlayer.Normalize(); // Normalize to get direction vector

		// Add variability in the flee direction by rotating around the y-axis
		float fleeAngle = Random.Range(-30.0f, 30.0f); // Adjust angle range as needed
		Quaternion rotation = Quaternion.Euler(0, fleeAngle, 0);
		Vector3 fleeDirection = rotation * fromPlayer;

		// Calculate potential flee position
		Vector3 randomFleeDirection = fleeDirection * monsterData.fleeDistance + monster.transform.position;

		NavMeshHit hit;
		if (NavMesh.SamplePosition(randomFleeDirection, out hit, monsterData.fleeDistance, NavMesh.AllAreas)) {
			destination = hit.position;
			agent.SetDestination(destination);
		} else {
			//Debug.Log("No valid navmesh point found in the flee direction!");
		}
	}

    public override void Exit()
    {
        base.Exit();
		animator.SetBool("IsRunning", false);
		if (agent.isActiveAndEnabled)
        {
            agent.ResetPath();
        }
    }
}
