using System.Collections;
using UnityEngine;

public class ExploringState : MonsterState
{
    private Transform targetArea;
    private Vector3 nextPosition;
    private float timer;

    public ExploringState(GameObject monster, MonsterData data, Transform targetArea) : base(monster, data)
    {
        this.targetArea = targetArea;
        ChooseNextPosition();
    }

    public override void Enter()
    {
        base.Enter();
        navMeshAgent.speed = monsterData.moveSpeed;
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

    private void ChooseNextPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * monsterData.exploringRadius;
        randomDirection += targetArea.position;
        UnityEngine.AI.NavMeshHit hit;
        UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, monsterData.exploringRadius, UnityEngine.AI.NavMesh.AllAreas);
        nextPosition = hit.position;
        navMeshAgent.SetDestination(nextPosition);
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
            timer -= Time.deltaTime;
        }
    }
}
