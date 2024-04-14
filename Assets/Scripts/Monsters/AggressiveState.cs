using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggressiveState : MonsterState
{

    public AggressiveState(GameObject monster, MonsterData monsterData) : base(monster, monsterData)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Monster becomes aggressive!");
    }

    public override void Execute()
    {
        // Example movement towards the player
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (Vector3.Distance(monster.transform.position, playerTransform.position) > monsterData.detectionRadius)
        {
            monster.GetComponent<MonsterController>().ChangeState(new ExploringState(monster, monsterData, monster.GetComponent<MonsterController>().explorationTarget));
        } else
        {
            float step = monsterData.moveSpeed * Time.deltaTime;  // Adjust speed as necessary
            monster.transform.position = Vector3.MoveTowards(monster.transform.position, playerTransform.position, step);
        }
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Monster stops being aggressive.");
    }
}