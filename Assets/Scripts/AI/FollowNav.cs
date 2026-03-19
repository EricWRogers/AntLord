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
        if (leader != null) leader.followers.Add(myAgent);
    }

    void Update()
    {
        if (myAgent == null || !myAgent.isOnNavMesh) return;
        if (leader == null) return;

        if (myAgent.remainingDistance >= 2f)
            transform.LookAt(myAgent.steeringTarget);

        if (recentCollision != null)
        {
            if (Vector3.Distance(transform.position, recentCollision.position) > transform.localScale.x)
                recentCollision = null;
        }

        if (leader.crumbs.Count != 0 && Vector3.Distance(transform.position, leader.transform.position) > leaderTail)
        {
            // Clamp crumbTrack just in case
            crumbTrack = Mathf.Clamp(crumbTrack, 0, leader.crumbs.Count - 1);

            myAgent.destination = leader.crumbs[crumbTrack];

            finalCloseDist = closeEnough * (leader.followers.Count * closeEnoughModifier);
            finalCloseDist = Mathf.Clamp(finalCloseDist, 1f, 2.5f);

            if (crumbTrack < leader.crumbs.Count - 1 && myAgent.remainingDistance < finalCloseDist)
                crumbTrack++;

            HandleAgentCollisions();
        }
    }
}