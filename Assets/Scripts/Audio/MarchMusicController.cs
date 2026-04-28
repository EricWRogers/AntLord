using UnityEngine;

public class MarchMusicController : MonoBehaviour
{
    [Header("AudioManager2 Sound Name")]
    public string marchSoundName = "AntMarch";

    [Header("Detect movement")]
    public float velocityEpsilon = 0.05f;      
    public float stillGraceSeconds = 0.50f;     
    public bool onlyPlayerTeam = true;           

    float lastMoveTime = -999f;

    void Update()
    {
        if (AudioManager2.instance == null) return;

        if (AnyFriendlyAntMoving())
            lastMoveTime = Time.time;

        bool shouldPlay = (Time.time - lastMoveTime) <= stillGraceSeconds;

        if (shouldPlay)
            AudioManager2.instance.Play(marchSoundName);
        else
            AudioManager2.instance.Stop(marchSoundName);
    }

    bool AnyFriendlyAntMoving()
    {
        GameObject[] ants = GameObject.FindGameObjectsWithTag("Ant");
        for (int i = 0; i < ants.Length; i++)
        {
            GameObject ant = ants[i];
            if (ant == null) continue;

            if (onlyPlayerTeam)
            {
                AntBrain brain = ant.GetComponent<AntBrain>();
                if (brain != null && brain.antType != null && brain.antType.teamID != 0)
                    continue;
            }

            CharacterController cc = ant.GetComponent<CharacterController>();
            if (cc == null) continue;

            
            Vector3 v = cc.velocity;
            v.y = 0f;

            if (v.magnitude > velocityEpsilon)
                return true;
        }

        return false;
    }
}