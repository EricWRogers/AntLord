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
        AntBrain brain = GetComponent<AntBrain>();
        bool isAI = brain != null && brain.antType.teamID != 0;

        // 1. TARGET ACQUISITION (The "Find" Logic)
        if (isAI && target == null)
        {
            if (task == AntTask.Food) 
            {
                target = FindFood();
                if (target != null) Debug.Log($"<color=green>{name} found food at {target.position}</color>");
         }
            else if (task == AntTask.Manual) 
            {
                target = home;
            }
    }

        // 2. MOVEMENT LOGIC
        if (target != null)
        {
            myAgent.SetDestination(target.position);

            if (myAgent.remainingDistance >= 1.5f)
            {
                Vector3 direction = myAgent.steeringTarget - transform.position;
                direction.y = 0; 
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
                }
            }

            // Crumb System
            crumbDropTimer += Time.deltaTime;
            if (crumbDropTimer >= crumbDropDelay)
            {
                crumbDropTimer = 0f;
                crumbs.Add(transform.position);
            }

            // Separation Logic
            HandleAgentCollisions();

            // 3. STATE TRANSITIONS
            if (task == AntTask.Food)
            {
                int requiredFood = (followers.Count * antTier) + (1 * antTier);
                if (foodBits >= requiredFood && target != home)
                {
                    target = home;
                    Debug.Log($"{name}: Full! Returning home.");
                }
            }
        }
        else
        {
            if(myAgent.hasPath) myAgent.ResetPath();
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
