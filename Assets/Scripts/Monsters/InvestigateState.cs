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

	public InvestigateState(GameObject monster, MonsterData monsterData, Vector3 soundPosition) : base(monster, monsterData) {
		animator = monster.GetComponentInChildren<Animator>();
		agent = monster.GetComponent<NavMeshAgent>();
		this.soundPosition = soundPosition;
		monsterController = monster.GetComponent<MonsterController>();
	}

	public override void Enter() {
		animator.SetBool("IsWalking", true);
		animator.SetBool("IsRunning", false);
		animator.SetBool("IsIdle", false);
		agent.SetDestination(soundPosition);
		navMeshAgent.speed = monsterData.walkSpeed;
		navMeshAgent.angularSpeed = monsterData.turnSpeed;
		investigateTimer = monsterData.pauseTime;
	}

	int interval = 10;
	int framesUntilNextInterval = 0;
	public override void Execute() {
		if (framesUntilNextInterval % interval == 0) {
			bool playerVisible = monsterController.IsPlayerVisible(monsterData.detectionRadiusInvestigating);
			if (playerVisible) {
				monsterController.ChangeState(new AggressiveState(monster, monsterData));
			}
		}
		framesUntilNextInterval++;

		if (!IsLookingAround) {
			LookInRandomDirection(); // Continue to update the looking direction during the pause
		}

		if (Vector3.Distance(monster.transform.position, soundPosition) < monsterData.investigationDistance) {
			investigateTimer += Time.deltaTime;
		}

		if (investigateTimer >= monsterData.investigationTime) {
			monsterController.ChangeState(new ExploringState(monster, monsterData, monsterController.explorationTarget));
		}

		if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= monsterData.stoppingDistance) {
			PauseAndLookAround();
		}
	}

	private void ChooseNextPosition() {
		Vector3 randomDirection = Random.insideUnitSphere * monsterData.exploringRadius;
		randomDirection += soundPosition;
		UnityEngine.AI.NavMeshHit hit;
		if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, monsterData.exploringRadius, UnityEngine.AI.NavMesh.AllAreas)) {
			float straightLineDistance = Vector3.Distance(monster.transform.position, hit.position);
			if (straightLineDistance <= monsterData.maxStraightLineDistance) {
				navMeshAgent.SetDestination(hit.position);
				animator.SetBool("IsWalking", true);
				animator.SetBool("IsIdle", false);
			} else {
				ChooseNextPosition(); // Recursively try again
			}
		}
	}

	private void PauseAndLookAround() {
		if (pauseTimer <= 0) {
			pauseTimer = monsterData.pauseTime; // Reset pause timer

			// Perform a random look around before moving again
			LookInRandomDirection();
			ChooseNextPosition();
		} else {
			if (!IsLookingAround) {
				StartLookingAround(); // Start the look around process if not already looking around
			}

			animator.SetBool("IsIdle", true);
			animator.SetBool("IsWalking", false);
			pauseTimer -= Time.deltaTime;
		}
	}

	private bool IsLookingAround = false;
	private float lookAroundTimer = 0;
	private Quaternion targetRotation;

	private void StartLookingAround() {
		IsLookingAround = true;
		lookAroundTimer = 3.0f; // Adjust time to look around
		targetRotation = Quaternion.Euler(0, Random.Range(-180, 180), 0); // Random rotation around y-axis
	}

	private void LookInRandomDirection() {
		if (IsLookingAround) {
			if (lookAroundTimer > 0) {
				// Smoothly rotate towards the target rotation
				monster.transform.rotation = Quaternion.Slerp(monster.transform.rotation, targetRotation, Time.deltaTime * monsterData.turnSpeed);
				lookAroundTimer -= Time.deltaTime;
			} else {
				// Stop looking around
				IsLookingAround = false;
			}
		}
	}

	public override void Exit() {
		base.Exit();
	}
}
