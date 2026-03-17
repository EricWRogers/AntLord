using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshSnapOnStart : MonoBehaviour
{
    public float snapRadius = 5f;

    void Start()
    {
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, snapRadius, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }
    }
}