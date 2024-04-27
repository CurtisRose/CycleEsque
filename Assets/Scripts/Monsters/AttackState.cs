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
        animator.SetBool("IsAttacking", true);
        animator.Play("Attack");
        monster.GetComponent<MonsterController>().SetTarget(playerTransform);
    }

    public override void Execute()
    {
        if (playerTransform == null) {
			monster.GetComponent<MonsterController>().ChangeState(new ExploringState(monster, monsterData, monster.GetComponent<MonsterController>().explorationTarget));
			return;
		}

        // Ensure the monster faces the player while attacking
        monster.transform.LookAt(playerTransform);

        // Check if the player has moved out of attack range
        if (Vector3.Distance(monster.transform.position, playerTransform.position) > monsterData.attackRange)
        {
            // If the player is out of attack range, switch back to aggressive state
            monster.GetComponent<MonsterController>().ChangeState(new AggressiveState(monster, monsterData));
        }
    }

    public override void Exit()
    {
        base.Exit();
        agent.isStopped = false;  // Allow the monster to move again
        animator.SetBool("IsAttacking", false);
    }
}