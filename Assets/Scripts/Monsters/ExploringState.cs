using System.Collections;
using UnityEngine;

public class ExploringState : MonsterState
{
    private Transform targetArea;
    private Vector3 nextPosition;
    private float timer;
    Animator animator;

    public ExploringState(GameObject monster, MonsterData data, Transform targetArea) : base(monster, data)
    {
        this.targetArea = targetArea;
        animator = monster.GetComponent<Animator>();
        monster.GetComponent<Health>().OnDamageTaken += OnDamageTaken;
        animator = monster.GetComponentInChildren<Animator>();
        animator.SetBool("IsIdle", true);
        navMeshAgent.speed = monsterData.walkSpeed;
        ChooseNextPosition();
    }

    public override void Enter()
    {
        base.Enter();
        navMeshAgent.speed = monsterData.walkSpeed;
        navMeshAgent.angularSpeed = monsterData.turnSpeed;
        timer = monsterData.pauseTime;
        ChooseNextPosition();
    }

    public override void Execute()
    {
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            PauseAndLookAround();
        }
    }

    public override void Exit()
    {
        base.Exit();
        // Unsubscribe to avoid memory leaks
        monster.GetComponent<Health>().OnDamageTaken -= OnDamageTaken;
    }

    private void ChooseNextPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * monsterData.exploringRadius;
        randomDirection += targetArea.position;
        UnityEngine.AI.NavMeshHit hit;
        UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, monsterData.exploringRadius, UnityEngine.AI.NavMesh.AllAreas);
        nextPosition = hit.position;
        navMeshAgent.SetDestination(nextPosition);
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsIdle", false);
        animator.Play("Walk");
    }

    private void OnDamageTaken(float damageAmount, float currentHealth)
    {
        // Change state to Aggressive when taking damage
        monster.GetComponent<MonsterController>().ChangeState(new AggressiveState(monster, monsterData));
    }

    private void PauseAndLookAround()
    {
        if (timer <= 0)
        {
            timer = monsterData.pauseTime * 2;  // Pause twice the time before moving again
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
