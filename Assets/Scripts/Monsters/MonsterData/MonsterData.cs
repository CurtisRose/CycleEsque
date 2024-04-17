using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Monsters/BasicMonster", order = 1)]
public class MonsterData : ScriptableObject
{
    public float walkSpeed;
    public float runSpeed;
    public float turnSpeed;
    public float stoppingDistance;
    public float acceleration;
    public float exploringRadius;
    public float pauseTime;
    public float detectionRadius;
    public float fleeDistance;
    public float minimumAggressionTime;
    public float health;
    public float armor;
    public float attackRange;
    public float attackDamage;
}
