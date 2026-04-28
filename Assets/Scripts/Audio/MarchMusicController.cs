using UnityEngine;

public class MarchMusicController : MonoBehaviour
{
    [Header("AudioManager2 Sound Names")]
    public string marchSoundName = "AntMarch";

    [Header("Stop rule")]
    public float leaderArriveDistance = 1.0f;

    [Header("Team Filter")]
    public bool onlyPlayerTeam = true; 

    void Update()
    {
        if (AudioManager2.instance == null) return;

        bool shouldPlay = AnySquadStillMarching();

        if (shouldPlay)
            AudioManager2.instance.Play(marchSoundName);
        else
            AudioManager2.instance.Stop(marchSoundName);
    }

    bool AnySquadStillMarching()
    {
        LeadNav[] leaders = FindObjectsByType<LeadNav>(FindObjectsSortMode.None);

        for (int i = 0; i < leaders.Length; i++)
        {
            LeadNav leader = leaders[i];
            if (leader == null || !leader.enabled) continue;
            if (leader.target == null) continue;

            
            if (onlyPlayerTeam)
            {
                AntBrain brain = leader.GetComponent<AntBrain>();
                if (brain != null && brain.antType != null && brain.antType.teamID != 0)
                    continue;
            }

            
            float dist = Vector3.Distance(leader.transform.position, leader.target.position);
            if (dist > leaderArriveDistance)
                return true;

            
            if (leader.followers != null)
            {
                for (int f = 0; f < leader.followers.Count; f++)
                {
                    FollowNav follower = leader.followers[f];
                    if (follower == null) continue;

                   
                    if (!follower.IsSettled)
                        return true;
                }
            }
        }

        return false;
    }
}