using UnityEngine;

public class FollowNav : NavParent
{
    public LeadNav leader;
    public float closeEnough = 0.25f;
    public float closeEnoughModifier = 0.5f;
    private float finalCloseDist;
    public float leaderTail = 0.3f;
    public int crumbTrack = 0;

   
    [Header("Arrival Formation")]
    public float formationStartDist = 2.0f; // begin spreading when close to leader
    public float formationRadius = 0.9f;    // ring radius around leader

   
    [Header("Settle")]
    public float settleDist = 0.30f;        
    public float unsettleDist = 0.60f;      

   
    [Header("Unstick Burst")]
    public float crowdCheckRadius = 0.25f;   // how close is overlapping
    public float scootDuration = 0.20f;      // how long scoot when crowded
    public float scootCooldown = 0.25f;      
    public float scootSteerStrength = 1.0f;  

   
    int slotSeed;

    
    bool isSettled = false;
    Vector3 settledSlot;

   
    int lastCrumbCount = 0;

    
    float scootUntil = 0f;
    float nextScootAllowed = 0f;
    Vector3 scootDir = Vector3.zero;

    public override void Start()
    {
        base.Start();

        // Old: if (leader != null) leader.followers.Add(myAgent);
        // New: followers list is FollowNav components
        if (leader != null) leader.followers.Add(this);

        
        slotSeed = Mathf.Abs(gameObject.GetInstanceID());
    }

    void Update()
    {
        if (mover == null) return;
        if (leader == null) return;

       
        if (leader.crumbs != null)
        {
            if (leader.crumbs.Count < lastCrumbCount)
            {
                isSettled = false;
            }
            lastCrumbCount = leader.crumbs.Count;
        }

        
        if (isSettled)
        {
            Vector3 a = transform.position; a.y = 0f;
            Vector3 b = settledSlot; b.y = 0f;

            if (Vector3.Distance(a, b) > unsettleDist)
            {
                isSettled = false;
            }
            else
            {
                

                mover.ClearGoal();

                bool crowded = IsCrowded();

                // Start a new scoot burst if allowed
                if (crowded && Time.time >= nextScootAllowed && Time.time >= scootUntil)
                {
                    
                    Vector3 sep = ComputeRawSeparationDirection();
                    if (sep.sqrMagnitude > 0.0001f)
                    {
                        scootDir = sep.normalized;
                        scootUntil = Time.time + scootDuration;
                        nextScootAllowed = Time.time + scootCooldown;
                    }
                }

                
                if (Time.time < scootUntil && scootDir.sqrMagnitude > 0.0001f)
                {
                    mover.SetSteering(scootDir * scootSteerStrength);
                }
                else
                {
                    // Otherwise fully still
                    mover.SetSteering(Vector3.zero);
                }

                return;
            }
        }

        // Old: rotate to steering target when moving
        // New: AntMover already handles rotation toward goal.

        if (recentCollision != null)
        {
            if (Vector3.Distance(transform.position, recentCollision.position) > transform.localScale.x)
                recentCollision = null;
        }

        
        float distToLeader = Vector3.Distance(transform.position, leader.transform.position);
        bool nearLeader = distToLeader <= formationStartDist;
        bool nearEndOfCrumbs = leader.crumbs != null && leader.crumbs.Count > 0 && crumbTrack >= leader.crumbs.Count - 2;

        if (nearLeader && nearEndOfCrumbs)
        {
            // Compute a slot around the leader
            Vector3 slot = ComputeStableSlot();

            // Use normal separation steering while moving into the slot
            HandleAgentCollisions();

            // Once close enough, latch this slot and stop updating goals
            Vector3 a = transform.position; a.y = 0f;
            Vector3 b = slot; b.y = 0f;

            if (Vector3.Distance(a, b) <= settleDist)
            {
                isSettled = true;
                settledSlot = slot;

                // Reset burst state
                scootUntil = 0f;
                nextScootAllowed = 0f;
                scootDir = Vector3.zero;

                mover.ClearGoal();
                mover.SetSteering(Vector3.zero);
                return;
            }

            mover.SetGoal(slot);
            return;
        }

        // Normal crumb follow behavior
        if (leader.crumbs.Count != 0 &&
            Vector3.Distance(transform.position, leader.transform.position) > leaderTail)
        {
            // Clamp crumbTrack just in case
            crumbTrack = Mathf.Clamp(crumbTrack, 0, leader.crumbs.Count - 1);

            // Old: myAgent.destination = leader.crumbs[crumbTrack];
            mover.SetGoal(leader.crumbs[crumbTrack]);

            finalCloseDist = closeEnough * (leader.followers.Count * closeEnoughModifier);
            finalCloseDist = Mathf.Clamp(finalCloseDist, 1f, 2.5f);

            
            Vector3 a = transform.position; a.y = 0;
            Vector3 b = leader.crumbs[crumbTrack]; b.y = 0;

            if (crumbTrack < leader.crumbs.Count - 1 &&
                Vector3.Distance(a, b) < finalCloseDist)
                crumbTrack++;

            HandleAgentCollisions();
        }
        else
        {
            // If we're close to leader, still apply separation steering so we don't overlap.
            HandleAgentCollisions();
        }
    }

    Vector3 ComputeStableSlot()
    {
        
        float u = (slotSeed % 1000) / 1000f;
        float angle = u * Mathf.PI * 2f;

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * formationRadius;
        return leader.transform.position + offset;
    }

    // NEW: detect real overlap with other ants
    bool IsCrowded()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, crowdCheckRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform == transform) continue;
            if (hits[i].CompareTag("Ant")) return true;
        }
        return false;
    }

    // NEW: compute a separation direction once
    Vector3 ComputeRawSeparationDirection()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, separationRadius);
        Vector3 sep = Vector3.zero;

        foreach (Collider c in nearby)
        {
            if (c.transform == transform) continue;
            if (!c.CompareTag("Ant")) continue;

            Vector3 away = transform.position - c.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude > 0.0001f)
                sep += away.normalized;
        }

        return sep;
    }
}