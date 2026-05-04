using System.Collections.Generic;
using UnityEngine;

public class MarchMusicController : MonoBehaviour
{
    public string marchSoundName = "AntMarch";

    [Header("Friendly filter")]
    public bool onlyPlayerTeam = true;

    [Header("Movement Detection")]
    public float velocityEpsilon = 0.03f;
    public float posEpsilon = 0.004f;
    public float stillGraceSeconds = 0.75f;

    readonly Dictionary<int, Vector3> lastPos = new Dictionary<int, Vector3>();
    float lastMoveTime = -999f;

    void Start()
    {
        if (AudioManager2.instance != null)
        {
            
            AudioManager2.instance.Play(marchSoundName);
            AudioManager2.instance.SetVolume(marchSoundName, 0f);
        }
    }

    void Update()
    {
        if (AudioManager2.instance == null) return;

        if (AnyFriendlyAntMoving())
        {
            lastMoveTime = Time.time;
        }

        bool shouldBeAudible = (Time.time - lastMoveTime) <= stillGraceSeconds;

        if (shouldBeAudible)
        {
            
            AudioManager2.instance.Play(marchSoundName);

            
            AudioManager2.instance.RestoreVolume(marchSoundName);
        }
        else
        {
           
            AudioManager2.instance.SetVolume(marchSoundName, 0f);
        }
    }

    bool AnyFriendlyAntMoving()
    {
        GameObject[] ants = GameObject.FindGameObjectsWithTag("Ant");
        bool movedByPos = false;

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

            if (cc != null)
            {
                Vector3 v = cc.velocity;
                v.y = 0f;

                if (v.sqrMagnitude > velocityEpsilon * velocityEpsilon)
                    return true;
            }

            int id = ant.GetInstanceID();
            Vector3 p = ant.transform.position;

            if (lastPos.TryGetValue(id, out Vector3 prev))
            {
                float dx = p.x - prev.x;
                float dz = p.z - prev.z;

                if ((dx * dx + dz * dz) > posEpsilon * posEpsilon)
                    movedByPos = true;

                lastPos[id] = p;
            }
            else
            {
                lastPos[id] = p;
            }
        }

        return movedByPos;
    }
}