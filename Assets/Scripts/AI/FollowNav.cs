using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class FollowNav : MonoBehaviour
{
    private NavMeshAgent myAgent;
    public LeadNav leader;
    private int crumbTrack = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // Using distance calculation constantly is bad for performance, need alternate method
        if(!leader.GetComponent<NavMeshAgent>().isStopped && leader.crumbs.Count != 0 && Vector3.Distance(transform.position, leader.transform.position) > 0.5f)
        {
            myAgent.destination = leader.crumbs[crumbTrack];

            if(crumbTrack < leader.crumbs.Count - 1 && Vector3.Distance(transform.position, leader.crumbs[crumbTrack]) < 0.1f)
            {
                crumbTrack++;
            }
        }
    }
}
