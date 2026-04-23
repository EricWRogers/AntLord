using UnityEngine;

public class LeaderMarchAudio : MonoBehaviour
{
    public string marchSoundName = "AntMarch";
    public float stopDistance = 2.0f;

    LeadNav lead;

    void Awake() => lead = GetComponent<LeadNav>();

    void Update()
    {
        if (AudioManager2.instance == null) return;
        if (lead == null || !lead.enabled || lead.target == null) return;

        float dist = Vector3.Distance(transform.position, lead.target.position);
        if (dist <= stopDistance)
            AudioManager2.instance.Stop(marchSoundName);
    }
}