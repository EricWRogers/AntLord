using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class LeadNav : NavParent
{
    public List<NavMeshAgent> followers;
    public GameObject crumbPrefab;
    public List<Vector3> crumbs;
    public float crumbDropDelay = 1f;
    private float crumbDropTimer = 0f;
    public Transform target;
    public Transform recentObjective;
    public Transform home;
    public int foodBits = 0;
    public AntTask task = AntTask.Manual;
    
    void Update()
    {
        if(target != null){
            
            if(myAgent.remainingDistance >= 2)
                transform.LookAt(myAgent.steeringTarget);

                crumbDropTimer += Time.deltaTime;

                if(crumbDropTimer >= crumbDropDelay)
                {
                    crumbDropTimer = 0f;
                    
                    crumbs.Add(transform.position);
                }

                // Set destination normally
                myAgent.destination = target.position;

                // Check for nearby agents and apply separation
                HandleAgentCollisions();

            if(foodBits >= (followers.Count * antTier) + (1 * antTier))
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

            if(foodCount >= (followers.Count * antTier)) //+ (1 * antTeir))
            {
                Transform foodTransform = FindFood();

                if(foodTransform != null)
                    target = foodTransform;
                else
                    Debug.Log("Failed to find food, freak out!");
            }
        }
    }
}
