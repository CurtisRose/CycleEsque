using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Monsters/BasicMonster", order = 1)]
public class MonsterData : ScriptableObject
{
    public float moveSpeed = 3f;
    public float turnSpeed = 120f;
    public float exploringRadius = 10f;
    public float pauseTime = 2f;
    public float detectionRadius = 15f;
    public float fleeDistance = 20f;
    public float minimumAggressionTime = 30f;
}
