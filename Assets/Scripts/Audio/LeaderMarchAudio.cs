using UnityEngine;

public class LeaderMarchAudio : MonoBehaviour
{
    public string marchSoundName = "AntMarch";
    public float stopDistance = 2.0f;

    [Header("Follower Check")]
    public bool waitForFollowers = true;

    LeadNav lead;

    void Awake()
    {
        lead = GetComponent<LeadNav>();
    }

    void Update()
    {
        if (AudioManager2.instance == null) return;
        if (lead == null || !lead.enabled || lead.target == null) return;

        float dist = Vector3.Distance(transform.position, lead.target.position);

        // Leader has not reached the objective yet, so do not stop the music
        if (dist > stopDistance)
            return;

        
        if (!waitForFollowers)
        {
            AudioManager2.instance.Stop(marchSoundName);
            return;
        }

      
        if (AllFollowersSettled())
        {
            AudioManager2.instance.Stop(marchSoundName);
        }
    }

    bool AllFollowersSettled()
    {
        if (lead.followers == null || lead.followers.Count == 0)
            return true;

        foreach (FollowNav follower in lead.followers)
        {
            if (follower == null)
                continue;

            if (!follower.IsSettled)
                return false;
        }

        return true;
    }
}