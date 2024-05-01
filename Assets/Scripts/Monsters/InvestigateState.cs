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

		// If the monster has gotten near the sound, start incrementing the investigate timer
		if (Vector3.Distance(monster.transform.position, soundPosition) < monsterData.investigationDistance) {
			investigateTimer += Time.deltaTime;
			animator.SetBool("IsIdle", true);
			animator.SetBool("IsWalking", false);
		}
	}

	public override void Exit() {
		base.Exit();
	}
}
