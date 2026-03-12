using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum AntTask {Manual, Food, Fight}

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
    //public bool arrived = false;
    public float separationRadius = 2f;
    public float separationForce = 5f;
    public int foodBits = 0;
    public bool amCarryingFood = false;
    public int antTeir = 1;
    public AntTask task = AntTask.Manual;
    //private bool checkingFood = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myAgent = GetComponent<NavMeshAgent>();
        myAgent.isStopped = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(target != null){
            
            if(myAgent.remainingDistance >= 2)
                transform.LookAt(myAgent.steeringTarget);

            //if(!arrived){
                crumbDropTimer += Time.deltaTime;

                if(crumbDropTimer >= crumbDropDelay)
                {
                    crumbDropTimer = 0f;
                    
                    //Debug crumb
                    //crumbs.Add(Instantiate(crumbPrefab, transform.position, Quaternion.identity).transform.position);
                    
                    //invisible crumb
                    crumbs.Add(transform.position);
                }

                // Set destination normally
                myAgent.destination = target.position;

                // Check for nearby agents and apply separation
                HandleAgentCollisions();
            //}

            // else if (arrived && !checkingFood)
            // {
            //     StartCoroutine(DelayedFoodCheck());
            // }

            if(foodBits >= (followers.Count * antTeir) + (1 * antTeir))
            {
                target = home;
            }


        }
    }

    Transform FindFood()
    {
        float sphereRadius = 25f;
        Collider[] hits = Physics.OverlapSphere(transform.position, sphereRadius);
        bool targetYet = false;
        int tries = 1;

        while(!targetYet && sphereRadius <= 500)
        {

            foreach(Collider col in hits)
            {
                if (col.CompareTag("Food"))
                {
                    Debug.Log("Found food to target!");
                    return col.transform;
                }
            }

            sphereRadius *= 2;

            if(sphereRadius <= 500) // should be relative to map size
            {
                Debug.Log($"Going for try {++tries}"); 
                hits = Physics.OverlapSphere(transform.position, sphereRadius);
            }
            else
                Debug.Log("Gave up on finding food");

        }


        return null; // this means you found no food
    }

    void FixedUpdate()
    {
        if(task == AntTask.Food && target == home && myAgent.remainingDistance <= 2)
        {
            int foodCount = 0;
            foreach (NavMeshAgent follower in followers)
            {
                if(follower.GetComponent<FollowNav>().amCarryingFood)
                    break;
                else
                    foodCount++;
            }

            if(foodCount >= (followers.Count * antTeir)) //+ (1 * antTeir))
            {
                Transform foodTransform = FindFood();

                if(foodTransform != null)
                    target = foodTransform;
                else
                    Debug.Log("Failed to find food, freak out!");
            }
        }
    }

    // public void DoneWithFood()
    // {
    //     //arrived = false;
    //     target = home;
    // }

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

    // private IEnumerator DelayedFoodCheck()
    // {
    //     checkingFood = true;

    //     while(checkingFood)
    //     {
    //         foreach(NavMeshAgent follower in followers)
    //         {
                
    //         }

    //         yield return new WaitForSeconds(1f);
    //     }

    //     //myAgent.destination = transform.position;
    //     Transform foodTransform = FindFood();

    //     if(foodTransform != null)
    //         myAgent.destination = foodTransform.position;
    //     else
    //         Debug.Log("Failed to find food, freak out!");

    //     foodBits = 0;
    // }
}
