using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshPathVisualizer : MonoBehaviour
{
    private NavMeshAgent agent;
	[SerializeField] bool DrawGizmos;

	void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void OnDrawGizmos()
    {
        if (!DrawGizmos)
			return;
        if (agent == null || agent.path == null)
            return;

        Gizmos.color = Color.red;
        Vector3 previousCorner = transform.position;

        foreach (Vector3 corner in agent.path.corners)
        {
            Gizmos.DrawLine(previousCorner, corner);
            Gizmos.DrawSphere(corner, 0.1f); // Draw small spheres at path corners
            previousCorner = corner;
        }
    }
}
