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

   
    int slotSeed;

    
    bool isSettled = false;
    Vector3 settledSlot;

   
    int lastCrumbCount = 0;

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
                mover.SetSteering(Vector3.zero);
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
            // Compute a stable slot around the leader
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
}