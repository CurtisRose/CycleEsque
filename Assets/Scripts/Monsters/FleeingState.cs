using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeingState : MonsterState
{
    public FleeingState(GameObject monster, MonsterData monsterData) : base(monster, monsterData)
    {
    }

    public override void Execute()
    {
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        float step = monsterData.moveSpeed * Time.deltaTime;  // Adjust speed for fleeing
        Vector3 fleeDirection = (monster.transform.position - playerTransform.position).normalized;
        monster.transform.position += fleeDirection * step;
    }
}