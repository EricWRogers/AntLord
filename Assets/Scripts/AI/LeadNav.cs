using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class LeadNav : MonoBehaviour
{
    public NavMeshAgent myAgent;
    public List<NavMeshAgent> followers;
    public GameObject crumbPrefab;
    public List<Vector3> crumbs;
    public float crumbDropDelay = 1f;
    private float crumbDropTimer = 0f;
    public Transform target;
    public Transform recentObjective;
    public Transform home;
    public bool arrived = false;
    public float separationRadius = 2f;
    public float separationForce = 5f;
    public int foodBits = 0;
    public bool amCarryingFood = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myAgent = GetComponent<NavMeshAgent>();
        myAgent.isStopped = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(myAgent.steeringTarget);

        if(!arrived){
            crumbDropTimer += Time.deltaTime;

            if(crumbDropTimer >= crumbDropDelay)
            {
                crumbDropTimer = 0f;
                
                //Debug crumb
                //crumbs.Add(Instantiate(crumbPrefab, transform.position, Quaternion.identity).transform.position);
                
                //invisible crumb
                crumbs.Add(transform.position);
            }

            // Set destination normallly
            myAgent.destination = target.position;

            // Check for nearby agents and apply separation
            HandleAgentCollisions();
        }

        else if (arrived)
        {
            foodBits = 0;
            // bool failed = false;
            // for(int i = 0; i < followers.Count; i++)
            // {
            //     for(int j = 0; i < followers[i].transform.childCount; j++)
            //     {
            //         if(followers[i].transform.GetChild(j).tag == "FoodBit")
            //         {
            //             foodBits++;
            //             break;
            //         }
            //         else if(j == followers[i].transform.childCount - 1)
            //             failed = true;
            //     }
            //     if (failed)
            //     {
            //         break;
            //     }
            // }


        }

        if(foodBits >= followers.Count + 1)
        {
            arrived = false;
            target = home;
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

    public void ReachedObjective()
    {
        if(!arrived)
        {
            Debug.Log("Hit target");
            arrived = true;
            crumbDropTimer = 0f;
            myAgent.isStopped = true;
        }
    }

    void FixedUpdate()
    {
        // Check if NavMeshAgent has reached the destination
        // if(!arrived && !myAgent.pathPending && myAgent.hasPath && myAgent.remainingDistance < 0.5f)
        // {
        //     Debug.Log("Hit target");
        //     arrived = true;
        //     crumbDropTimer = 0f;
        //     myAgent.isStopped = true;
        // }
    }

    // // never got called?
    // void OnTriggerEnter(Collider other)
    // {
    //     if(other.tag == "Finish")
    //     {
    //         Debug.Log("Hit target");
    //         myAgent.isStopped = true;
    //         crumbDropTimer = 0f;
    //     }
    // }
}
