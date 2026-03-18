using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class FollowNav : MonoBehaviour
{
    public NavMeshAgent myAgent;
    public LeadNav leader;
    public Transform recentCollision = null;

    public float closeEnough = 0.25f;
    public float closeEnoughModifier = 0.5f;
    private float finalCloseDist;

    public float leaderTail = 0.3f;
    public int crumbTrack = 0;

    public float separationRadius = 2f;
    public float separationForce = 5f;

    public bool amCarryingFood = false;
    public int antTier = 1;

    void Start()
    {
        myAgent = GetComponent<NavMeshAgent>();
        if (leader != null) leader.followers.Add(myAgent);
        myAgent.isStopped = false;
    }

    void Update()
    {
        if (myAgent == null || !myAgent.isOnNavMesh) return;
        if (leader == null) return;

        if (myAgent.remainingDistance >= 2f)
            transform.LookAt(myAgent.steeringTarget);

        if (recentCollision != null)
        {
            if (Vector3.Distance(transform.position, recentCollision.position) > transform.localScale.x)
                recentCollision = null;
        }

        if (leader.crumbs.Count != 0 && Vector3.Distance(transform.position, leader.transform.position) > leaderTail)
        {
            // Clamp crumbTrack just in case
            crumbTrack = Mathf.Clamp(crumbTrack, 0, leader.crumbs.Count - 1);

            myAgent.destination = leader.crumbs[crumbTrack];

            finalCloseDist = closeEnough * (leader.followers.Count * closeEnoughModifier);
            finalCloseDist = Mathf.Clamp(finalCloseDist, 1f, 2.5f);

            if (crumbTrack < leader.crumbs.Count - 1 && myAgent.remainingDistance < finalCloseDist)
                crumbTrack++;

            HandleAgentCollisions();
        }
    }

    void HandleAgentCollisions()
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