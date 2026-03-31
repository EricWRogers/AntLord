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

    void Start()
    {
        EnemyCommanderAI commander = Object.FindFirstObjectByType<EnemyCommanderAI>();
        AntBrain brain = GetComponent<AntBrain>();

        if (commander != null && brain != null && brain.antType.teamID == 1)
        {
            commander.RegisterNewSquad(this);
        }
    }
    
    void Update()
    {
        // 1. Identify if this is an AI-controlled Enemy Ant
        AntBrain brain = GetComponent<AntBrain>();
        bool isAI = brain != null && brain.antType.teamID != 0;

        // 2. AI TARGET ACQUISITION
        if (isAI && task == AntTask.Food && target == null)
        {
            Transform foundFood = FindFood();
        
            if (foundFood != null)
            {
                target = foundFood;
                recentObjective = foundFood;
            
                myAgent.isStopped = false;
                myAgent.SetDestination(target.position);
            
                Debug.Log($"<color=green>{name} targeting: {target.name} at {target.position}</color>");
            }
        }

        // 3. MOVEMENT & PATHING LOGIC
        if (target != null)
        {
            myAgent.SetDestination(target.position);

            if (myAgent.remainingDistance >= 1.0f)
            {
                Vector3 lookPos = myAgent.steeringTarget - transform.position;
                lookPos.y = 0;
                if (lookPos != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookPos);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }

            crumbDropTimer += Time.deltaTime;
            if (crumbDropTimer >= crumbDropDelay)
            {
                crumbDropTimer = 0f;
                crumbs.Add(transform.position);
            }

            HandleAgentCollisions();

        // 4. SCAVENGING STATE CHECK
            if (task == AntTask.Food)
            {
                int capacity = (followers.Count + 1) * antTier;
            
                if (foodBits >= capacity)
                {
                    if (home != null)
                    {
                        target = home;
                        recentObjective = home; 
                        Debug.Log($"<color=yellow>{name} squad is full! Returning to {home.name}</color>");
                    }
                }
            }
        }
        else if (isAI && recentObjective != null)
        {
            myAgent.SetDestination(recentObjective.position);
        
            if (myAgent.remainingDistance <= 1.0f)
            {
                recentObjective = null;
            }
        }
    }
    
    Transform FindFood()
    {
    float sphereRadius = 25f;
        float maxSearchRange = 500f;

        while (sphereRadius <= maxSearchRange)
       {
           Collider[] hits = Physics.OverlapSphere(transform.position, sphereRadius);
           Transform closestFood = null;
            float closestDistance = Mathf.Infinity;

            foreach (Collider col in hits)
           {
               if (col.CompareTag("Food"))
               {
                   float dist = Vector3.Distance(transform.position, col.transform.position);
                   if (dist < closestDistance)
                   {
                       closestDistance = dist;
                       closestFood = col.transform;
                   }
               }
            }

            if (closestFood != null)
            {
                recentObjective = closestFood; 
            
                Debug.Log($"<color=green>New Objective: {closestFood.name} at {closestFood.position}</color>");
                return closestFood;
            }

               sphereRadius *= 2;
    }
    return null;
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
