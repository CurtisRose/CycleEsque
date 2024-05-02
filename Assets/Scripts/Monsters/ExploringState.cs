using System.Collections;
using UnityEngine;

public class ExploringState : MonsterState
{
    private Transform targetArea;
    private Vector3 nextPosition;
    private float timer;
    Animator animator;
	private MonsterController monsterController;

	public ExploringState(GameObject monster, MonsterData data, Transform targetArea) : base(monster, data)
    {
        this.targetArea = targetArea;
        animator = monster.GetComponent<Animator>();
        animator = monster.GetComponentInChildren<Animator>();
        animator.SetBool("IsIdle", true);
        navMeshAgent.speed = monsterData.walkSpeed;
		monsterController = monster.GetComponent<MonsterController>();
	}

    public override void Enter()
    {
        base.Enter();
        navMeshAgent.speed = monsterData.walkSpeed;
        navMeshAgent.angularSpeed = monsterData.turnSpeed;
        timer = monsterData.pauseTime;
        ChooseNextPosition();
    }

	int interval = 10;
	int framesUntilNextInterval = 0;
	public override void Execute() {
		if (framesUntilNextInterval % interval == 0) {
			bool playerVisible = monsterController.IsPlayerVisible(monsterData.detectionRadiusExploring);
			if (playerVisible) {
				monsterController.ChangeState(new AggressiveState(monster, monsterData));
			}
		}
		framesUntilNextInterval++;

		// If arrived at the destination or very close
		if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= monsterData.stoppingDistance)
        {
            PauseAndLookAround();
        }

    }

	public override void Exit()
    {
        base.Exit();
    }

	private void ChooseNextPosition() {
		Vector3 randomDirection = Random.insideUnitSphere * monsterData.exploringRadius;
		randomDirection += targetArea.position;
		UnityEngine.AI.NavMeshHit hit;
		if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, monsterData.exploringRadius, UnityEngine.AI.NavMesh.AllAreas)) {
			float straightLineDistance = Vector3.Distance(monster.transform.position, hit.position);
			if (straightLineDistance <= monsterData.maxStraightLineDistance) {
				nextPosition = hit.position;
				navMeshAgent.SetDestination(nextPosition);
				animator.SetBool("IsWalking", true);
				animator.SetBool("IsIdle", false);
			} else {
				ChooseNextPosition(); // Recursively try again
			}
		}
	}

    private void PauseAndLookAround()
    {
        if (timer <= 0)
        {
            timer = monsterData.pauseTime;  // Pause twice the time before moving again
            ChooseNextPosition();
        }
        else
        {
            animator.SetBool("IsIdle", true);
            animator.SetBool("IsWalking", false);
            timer -= Time.deltaTime;
        }
    }
}
