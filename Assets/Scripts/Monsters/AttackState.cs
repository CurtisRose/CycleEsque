using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttackState : MonsterState
{
    private Animator animator;
    private Transform playerTransform;
    private NavMeshAgent agent;

    public AttackState(GameObject monster, MonsterData monsterData) : base(monster, monsterData)
    {
        animator = monster.GetComponentInChildren<Animator>();
        playerTransform = monster.GetComponent<MonsterController>().GetPlayers()[0].transform;
        agent = monster.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        base.Enter();
        agent.isStopped = true;  // Stop the monster from moving
		animator.ResetTrigger("AttackTrigger");  // Reset trigger when entering the state
		animator.SetTrigger("AttackTrigger");    // Set trigger to start the animation
		monster.GetComponent<MonsterController>().SetTarget(playerTransform);
    }

    public override void Execute()
    {
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
				// Only retrigger the attack if the animation has finished and the player is still in range
				animator.SetTrigger("AttackTrigger");
			}
		}
	}

    public override void Exit()
    {
        base.Exit();
        agent.isStopped = false;  // Allow the monster to move again
    }
}