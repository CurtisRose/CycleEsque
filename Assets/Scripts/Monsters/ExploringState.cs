using System.Collections;
using UnityEngine;

public class ExploringState : MonsterState
{
    private Transform targetArea;
    private Vector3 nextPosition;
    private float timer;

    public ExploringState(GameObject monster, MonsterData monsterData, Transform targetArea) : base(monster, monsterData)
    {
        this.targetArea = targetArea;
        ChooseNextPosition();
    }

    public override void Enter()
    {
        base.Enter();
        timer = monsterData.pauseTime; // Start with a pause
    }

    public override void Execute()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            if (Vector3.Distance(monster.transform.position, nextPosition) < 0.5f)
            {
                // When arrived at the target position, pause and look around
                PauseAndLookAround();
            }
            else
            {
                // Move towards the next position
                MoveToNextPosition();
            }
        }
    }

    private void ChooseNextPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * monsterData.exploringRadius;
        randomDirection += targetArea.position;
        randomDirection.y = monster.transform.position.y; // Keep the monster on the same plane
        nextPosition = randomDirection;
    }

    private void MoveToNextPosition()
    {
        monster.transform.position = Vector3.MoveTowards(monster.transform.position, nextPosition, monsterData.moveSpeed * Time.deltaTime);
        monster.transform.rotation = Quaternion.RotateTowards(monster.transform.rotation, Quaternion.LookRotation(nextPosition - monster.transform.position), monsterData.turnSpeed * Time.deltaTime);
    }

    private void PauseAndLookAround()
    {
        // Rotate in place
        monster.transform.Rotate(0, monsterData.turnSpeed * Time.deltaTime, 0);

        // Reset timer and choose a new position after the pause ends
        if (timer <= -monsterData.pauseTime)
        {
            ChooseNextPosition();
            timer = monsterData.pauseTime;
        }
    }
}
