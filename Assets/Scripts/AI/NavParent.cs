using UnityEngine;
using UnityEngine.AI;

public enum AntTask {Manual, Food, Fight}

[RequireComponent(typeof(NavMeshAgent))]
public abstract class NavParent : MonoBehaviour
{
    public NavMeshAgent myAgent;
    public Transform recentCollision = null;
    public float separationRadius = 2f;
    public float separationForce = 5f;
    public bool amCarryingFood = false;
    public int antTier = 1;

    public virtual void Start()
    {
        myAgent = GetComponent<NavMeshAgent>();
        myAgent.isStopped = false;
    }

    public void HandleAgentCollisions()
    {
        if (!myAgent.isOnNavMesh) return;

        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, separationRadius);
        Vector3 separationVector = Vector3.zero;

        foreach (Collider collider in nearbyColliders)
        {
            NavMeshAgent otherAgent = collider.GetComponent<NavMeshAgent>();
            if (otherAgent != null && otherAgent != myAgent)
            {
                Vector3 awayFromAgent = (transform.position - otherAgent.transform.position).normalized;
                separationVector += awayFromAgent;
            }
        }

        if (separationVector.sqrMagnitude > 0f)
        {
            Vector3 separatedDestination = myAgent.destination + separationVector.normalized * separationForce;
            myAgent.destination = separatedDestination;
        }
    }
}