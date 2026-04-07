using UnityEngine;

public class FollowNav : NavParent
{
    public LeadNav leader;
    public float closeEnough = 0.25f;
    public float closeEnoughModifier = 0.5f;
    private float finalCloseDist;
    public float leaderTail = 0.3f;
    public int crumbTrack = 0;

    public override void Start()
    {
        base.Start();

        // Old: if (leader != null) leader.followers.Add(myAgent);
        // New: followers list is FollowNav components
        if (leader != null) leader.followers.Add(this);
    }

    void Update()
    {
        if (mover == null) return;
        if (leader == null) return;

        // Old: rotate to steering target when moving
        // New: AntMover already handles rotation toward goal.

        if (recentCollision != null)
        {
            if (Vector3.Distance(transform.position, recentCollision.position) > transform.localScale.x)
                recentCollision = null;
        }

        if (leader.crumbs.Count != 0 &&
            Vector3.Distance(transform.position, leader.transform.position) > leaderTail)
        {
            // Clamp crumbTrack just in case
            crumbTrack = Mathf.Clamp(crumbTrack, 0, leader.crumbs.Count - 1);

            // Old: myAgent.destination = leader.crumbs[crumbTrack];
            mover.SetGoal(leader.crumbs[crumbTrack]);

            finalCloseDist = closeEnough * (leader.followers.Count * closeEnoughModifier);
            finalCloseDist = Mathf.Clamp(finalCloseDist, 1f, 2.5f);

            // Old: if (crumbTrack < ... && myAgent.remainingDistance < finalCloseDist) crumbTrack++;
            // New: compute 2D distance to current crumb
            Vector3 a = transform.position; a.y = 0;
            Vector3 b = leader.crumbs[crumbTrack]; b.y = 0;

            if (crumbTrack < leader.crumbs.Count - 1 &&
                Vector3.Distance(a, b) < finalCloseDist)
                crumbTrack++;

            HandleAgentCollisions();
        }
    }
}