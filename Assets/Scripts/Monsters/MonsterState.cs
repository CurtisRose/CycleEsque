using System.Collections;
using UnityEngine;

public abstract class MonsterState
{
    protected GameObject monster;
    protected MonsterData monsterData;

    // Constructor to initialize the state with the monster object
    public MonsterState(GameObject monster, MonsterData data)
    {
        this.monster = monster;
        this.monsterData = data;
    }

    // Called when the state is entered
    public virtual void Enter()
    {
        Debug.Log("Entering state: " + GetType().Name);
    }

    // Called each frame the state is active
    public abstract void Execute();

    // Called when exiting the state
    public virtual void Exit()
    {
        Debug.Log("Exiting state: " + GetType().Name);
    }
}