using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttackState : MonsterState
{
    private Animator animator;
    private Transform playerTransform;
    private NavMeshAgent agent;
	private Vector3 originalPosition;
	private bool isLungingForward = false;
	private MonsterController monsterController;
	float lungeDistance = 5f;
	float lungeSpeed = 10f;

	public AttackState(GameObject monster, MonsterData monsterData) : base(monster, monsterData)
    {
        animator = monster.GetComponentInChildren<Animator>();
        //playerTransform = monster.GetComponent<MonsterController>().GetPlayers()[0].transform;
        agent = monster.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        base.Enter();
        agent.isStopped = true;  // Stop the monster from moving
		animator.ResetTrigger("AttackTrigger");  // Reset trigger when entering the state
		animator.SetTrigger("AttackTrigger");    // Set trigger to start the animation
		monster.GetComponent<MonsterController>().SetTarget(playerTransform);
		originalPosition = monster.transform.localPosition;  // Store the original position for lunging
		monsterController = monster.GetComponent<MonsterController>();
	}

	public override void Execute() {
		if (Input.GetKey(KeyCode.L)) {
			LungeForward();
		}

		return;
		if (playerTransform == null) {
			monster.GetComponent<MonsterController>().ChangeState(new ExploringState(monster, monsterData, monster.GetComponent<MonsterController>().explorationTarget));
			return;
		}

		monster.transform.LookAt(playerTransform);

		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		bool isAnimationFinished = stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 0.7f;

		if (isAnimationFinished && !animator.IsInTransition(0)) {
			if (Vector3.Distance(monster.transform.position, playerTransform.position) > monsterData.attackRange) {
				MonsterController monsterController = monster.GetComponent<MonsterController>();
				if (monsterController.GetPlayers()[0] == null) {
					monsterController.ChangeState(new ExploringState(monsterController.gameObject, monsterData, monsterController.explorationTarget));
				} else {
					monster.GetComponent<MonsterController>().ChangeState(new AggressiveState(monster, monsterData));
				}
			} else {
				animator.SetTrigger("AttackTrigger");
			}
		}
	}

	public void StartLunge() {
		isLungingForward = true;
	}

	public void EndLunge() {
		isLungingForward = false;
		monster.transform.localPosition = originalPosition;
	}

	private void LungeForward() {
		Vector3 targetPosition = originalPosition + monster.transform.forward * lungeDistance;
		monster.transform.localPosition = Vector3.MoveTowards(monster.transform.localPosition, targetPosition, lungeSpeed * Time.deltaTime);

		if (monster.transform.localPosition == targetPosition) {
			isLungingForward = false;
			monsterController.StartStateCoroutine(LungeBack());
		}
	}

	private IEnumerator LungeBack() {
		Vector3 targetPosition = originalPosition;

		while (monster.transform.localPosition != targetPosition) {
			monster.transform.localPosition = Vector3.MoveTowards(monster.transform.localPosition, targetPosition, lungeSpeed * Time.deltaTime);
			yield return null;
		}
	}

	public override void Exit()
    {
        base.Exit();
        agent.isStopped = false;  // Allow the monster to move again
    }
}