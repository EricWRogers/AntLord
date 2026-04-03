using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyBaseTrigger : MonoBehaviour
{
    public EnemyCommanderAI commander;
    
    public int playerTeamID = 0; 

    void OnTriggerEnter(Collider other)
    {
        AntBrain invadingAnt = other.GetComponent<AntBrain>();

        if (invadingAnt != null && invadingAnt.antType.teamID == playerTeamID)
        {
            if (commander != null)
            {
                commander.TriggerDefenseMode();
            }
        }
    }
}