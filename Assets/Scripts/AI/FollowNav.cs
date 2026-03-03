using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class FollowNav : MonoBehaviour
{
    public NavMeshAgent myAgent;
    public LeadNav leader;
    public Transform recentCollision = null;
    public float closeEnough = 1f;
    public float leaderTail = 0.3f;
    public int crumbTrack = 0;
    public float separationRadius = 2f;
    public float separationForce = 5f;
    public bool amCarryingFood = false;
    public int antTier = 1;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myAgent = GetComponent<NavMeshAgent>();

        // temp add self to leader group
        if(leader != null)
            leader.followers.Add(myAgent);

        myAgent.isStopped = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(leader != null){

            if(myAgent.remainingDistance >= 2)
                transform.LookAt(myAgent.steeringTarget);
            
            // Supercede normal pathfinding if ants recently bumped
            if(recentCollision != null)
            {
                if(Vector3.Distance(transform.position, recentCollision.position) > transform.localScale.x)
                {
                    recentCollision = null;
                }
            }

            // Follow crumbs left by leader
            if(!leader.arrived && leader.crumbs.Count != 0 && Vector3.Distance(transform.position, leader.transform.position) > leaderTail)
            {
                myAgent.destination = leader.crumbs[crumbTrack];

                if(crumbTrack < leader.crumbs.Count - 1 && myAgent.remainingDistance < closeEnough)
                {
                    crumbTrack++;
                }

                // Check for nearby agents and apply separation
                HandleAgentCollisions();
            }
            else if(leader.arrived)
            {
                //myAgent.isStopped = true;

                myAgent.destination = leader.recentObjective.position;
                leader.crumbs.Clear();
                crumbTrack = 0;
            }
        }
    }

    void HandleAgentCollisions()
    {
        // Find all nearby agents within separationRadius
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, separationRadius);
        Vector3 separationVector = Vector3.zero;

        foreach (Collider collider in nearbyColliders)
        {
            NavMeshAgent otherAgent = collider.GetComponent<NavMeshAgent>();
            
            // If it's another agent (not self), apply separation
            if (otherAgent != null && otherAgent != myAgent)
            {
                Vector3 awayFromAgent = (transform.position - otherAgent.transform.position).normalized;
                separationVector += awayFromAgent;
            }
        }

        // Apply separation by slightly offsetting the destination
        if (separationVector.magnitude > 0)
        {
            Vector3 separatedDestination = myAgent.destination + separationVector.normalized * separationForce;
            myAgent.destination = separatedDestination;
        }
    }


}
