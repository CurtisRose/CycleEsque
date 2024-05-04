using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class InvestigateState : MonsterState {
	private Animator animator;
	private Vector3 soundPosition;
	private NavMeshAgent agent;
	private MonsterController monsterController;
	private float investigateTimer = 0f;
	private float pauseTimer;
	private float pauseDuration = 2.0f;

	public InvestigateState(GameObject monster, MonsterData monsterData, Vector3 soundPosition) : base(monster, monsterData) {
		animator = monster.GetComponentInChildren<Animator>();
		agent = monster.GetComponent<NavMeshAgent>();
		this.soundPosition = soundPosition;
		monsterController = monster.GetComponent<MonsterController>();
	}

	public override void Enter() {
		animator.SetBool("IsWalking", true);
		agent.SetDestination(soundPosition);
		navMeshAgent.speed = monsterData.walkSpeed;
		navMeshAgent.angularSpeed = monsterData.turnSpeed;
		investigateTimer = 0;
		pauseTimer = 0;
	}

	int interval = 10;
	int framesUntilNextInterval = 0;
	public override void Execute() {
		if (framesUntilNextInterval % interval == 0) {
			bool playerVisible = monsterController.IsPlayerVisible(monsterData.detectionRadiusInvestigating);
			if (playerVisible) {
				monsterController.ChangeState(new AggressiveState(monster, monsterData));
			}
			framesUntilNextInterval = 0;
		}
		framesUntilNextInterval++;

		if (Vector3.Distance(monster.transform.position, soundPosition) < monsterData.investigationDistance) {
			investigateTimer += Time.deltaTime;
		}

		if (investigateTimer >= monsterData.investigationTime) {
			monsterController.ChangeState(new ExploringState(monster, monsterData, monsterController.explorationTarget));
		}

		// Check if reached the current destination
		if (!agent.pathPending && agent.remainingDistance <= monsterData.stoppingDistance) {
			if (pauseTimer <= 0) {
				pauseTimer = pauseDuration; // Reset pause timer when destination is reached
			} else {
				pauseTimer -= Time.deltaTime;
				if (pauseTimer <= 0) {
					ChooseNextPosition(); // Only choose next position after pause
				}
			}
		}
	}

	public override void Exit() {
		animator.SetBool("IsWalking", false);
		animator.SetBool("IsIdle", false);
		base.Exit();
	}

	private void ChooseNextPosition() {
		Vector3 randomDirection = Random.insideUnitSphere * monsterData.investigationDistance;
		randomDirection += soundPosition;
		NavMeshHit hit;
		if (NavMesh.SamplePosition(randomDirection, out hit, monsterData.exploringRadius, NavMesh.AllAreas)) {
			float straightLineDistance = Vector3.Distance(monster.transform.position, hit.position);
			navMeshAgent.SetDestination(hit.position);
		}
	}
}
