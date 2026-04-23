using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class LeadNav : NavParent
{
    // Old: public List<NavMeshAgent> followers;
    // New: store follower components
    public List<FollowNav> followers = new List<FollowNav>();

    public GameObject crumbPrefab;
    public List<Vector3> crumbs;
    public float crumbDropDelay = 1f;
    private float crumbDropTimer = 0f;

    public Transform target;
    public Transform recentObjective;
    public Transform home;

    public int foodBits = 0;
    public AntTask task = AntTask.Manual;

    
    public VoxelAStar astar;

    
    private List<Vector3> path = new List<Vector3>();
    private int pathIndex = 0;
    private float repathCooldown = 0.5f;
    private float nextRepathTime = 0f;
    private Vector3 lastFlatGoal;

    override public void Start()
    {
        base.Start();

        EnemyCommanderAI commander = FindFirstObjectByType<EnemyCommanderAI>();
        AntBrain brain = GetComponent<AntBrain>();

        // Old: myAgent.updateRotation = false;
        // New: AntMover rotates manually already.

        if (!astar) astar = FindFirstObjectByType<VoxelAStar>();

        if (commander != null && brain != null && brain.antType.teamID == 1)
        {
            commander.RegisterNewSquad(this);
        }

        // NEW: leaders should be less affected by separation so they don't get surrounded
        separationSteerScale = 0.5f;
    }

    void Update()
    {
        // 1. Identify if this is an AI-controlled Enemy Ant
        AntBrain brain = GetComponent<AntBrain>();
        bool isAI = brain != null && brain.antType.teamID != 0;

        // 2. AI TARGET ACQUISITION
        if (isAI && (task == AntTask.Food || task == AntTask.Materials) && target == null)
        {
            Transform foundObjective = FindObjective();

            if (foundObjective != null)
            {
                target = foundObjective;
                recentObjective = foundObjective;

                // Old:
                // myAgent.isStopped = false;
                // myAgent.SetDestination(target.position);

                Debug.Log($"<color=green>{name} targeting: {target.name} at {target.position}</color>");
            }
        }

        // 3. MOVEMENT & PATHING LOGIC
        if (target != null)
        {
            // Old logic:
            // - SamplePosition to find actual navmesh surface under food
            // - Only SetDestination when needed
            //
            // New logic:
            // - Use A* over VoxelNavGrid to build a waypoint list
            // - Follow waypoints with AntMover

            Vector3 goal = target.position;

            // Flattened comparisons 
            Vector3 flatGoal = new Vector3(goal.x, 0, goal.z);

            bool needRepath = path.Count == 0 || pathIndex >= path.Count;
            bool goalMoved = Vector3.Distance(lastFlatGoal, flatGoal) > 1.0f;

            if (Time.time >= nextRepathTime && (needRepath || goalMoved))
            {
                nextRepathTime = Time.time + repathCooldown;
                lastFlatGoal = flatGoal;

                if (astar != null && astar.TryPath(transform.position, goal, out List<Vector3> newPath))
                {
                    path = newPath;
                    pathIndex = 0;
                }
                else
                {
                    // If no path found, still try direct 
                    path.Clear();
                    pathIndex = 0;
                }
            }

            // Follow path (or direct)
            if (mover != null)
            {
                if (path.Count > 0 && pathIndex < path.Count)
                {
                    Vector3 wp = path[pathIndex];
                    mover.SetGoal(wp);

                    Vector3 a = transform.position; a.y = 0;
                    Vector3 b = wp; b.y = 0;

                    if (Vector3.Distance(a, b) <= 0.6f)
                        pathIndex++;
                }
                else
                {
                    mover.SetGoal(goal);
                }
            }

            // 5. Drop crumbs for the followers 
            crumbDropTimer += Time.deltaTime;
            if (crumbDropTimer >= crumbDropDelay)
            {
                crumbDropTimer = 0f;
                crumbs.Add(transform.position);
            }

            // NEW: safe again (steering-based in NavParent)
            HandleAgentCollisions();

            // 4. SCAVENGING STATE CHECK (unchanged logic)
            if (task == AntTask.Food || task == AntTask.Materials)
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
            // Old:
            // myAgent.SetDestination(recentObjective.position);
            // if (myAgent.remainingDistance <= 1.0f) recentObjective = null;

            // New: treat recentObjective as a goal to move toward directly
            if (mover != null) mover.SetGoal(recentObjective.position);

            Vector3 a = transform.position; a.y = 0;
            Vector3 b = recentObjective.position; b.y = 0;

            if (Vector3.Distance(a, b) <= 1.0f)
            {
                recentObjective = null;
            }

            // Still apply separation steering
            HandleAgentCollisions();
        }
    }

    Transform FindObjective()
    {
        string tag;

        if(task == AntTask.Food)
            tag = "Food";
        else if(task == AntTask.Materials)
            tag = "Material";
        else
            return null;

        float sphereRadius = 25f;
        float maxSearchRange = 500f;

        while (sphereRadius <= maxSearchRange)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, sphereRadius);
            Transform closestFood = null;
            Transform closestMaterial = null;
            float closestDistance = Mathf.Infinity;

            foreach (Collider col in hits)
            {
                if (col.CompareTag(tag))
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

            if (closestMaterial != null)
            {
                recentObjective = closestMaterial;
                Debug.Log($"<color=green>New Objective: {closestMaterial.name} at {closestMaterial.position}</color>");
                return closestMaterial;
            }

            sphereRadius *= 2;
        }

        return null;
    }

    void FixedUpdate()
    {
        // Old condition: if(task == Food && target == home && myAgent.remainingDistance <= 2)
        // New condition: if close to home in flat distance
        if ((task == AntTask.Food || task == AntTask.Materials) && target == home && home != null)
        {
            Vector3 a = transform.position; a.y = 0;
            Vector3 b = home.position; b.y = 0;

            if (Vector3.Distance(a, b) <= 2f)
            {
                int foodCount = 0;
                foreach (FollowNav follower in followers)
                {
                    if (follower != null && follower.amCarryingFood)
                        break;
                    else
                        foodCount++;
                }

                if (foodCount >= (followers.Count * antTier))
                {
                    Transform foodTransform = FindObjective();

                    if (foodTransform != null)
                        target = foodTransform;
                    else
                        Debug.Log("Failed to find objective, freak out!");
                }
            }
        }
    }
}