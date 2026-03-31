using UnityEngine;
using System.Collections.Generic;

public enum CommanderState { Scavenging, Defending, AllOutAttack }
public enum EnemyPersonality { Aggressive, Balanced, Turtling }

public class EnemyCommanderAI : MonoBehaviour
{
    [Header("State & Personality")]
    public CommanderState currentState = CommanderState.Scavenging;
    public EnemyPersonality personality = EnemyPersonality.Balanced;

    [Header("Thresholds & Strategy")]
    public int armySizeThreshold; 
    public float timeUntilAttack; 
    private float elapsedTime = 0f;

    [Header("References")]
    public Transform playerBase;
    public Transform enemyBase;
    
    public List<LeadNav> activeEnemySquads = new List<LeadNav>();

    void Start()
    {
        ApplyPersonalityTraits();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        activeEnemySquads.RemoveAll(squad => squad == null);

        EvaluateStrategy();

        switch (currentState)
        {
            case CommanderState.Scavenging:
                HandleScavenging();
                break;
            case CommanderState.Defending:
                HandleDefending();
                break;
            case CommanderState.AllOutAttack:
                HandleAllOutAttack();
                break;
        }
    }

    private void EvaluateStrategy()
    {
        if (currentState == CommanderState.AllOutAttack) return;

        if (activeEnemySquads.Count >= armySizeThreshold || elapsedTime >= timeUntilAttack)
        {
            Debug.Log("<color=red>Enemy Commander: Threshold reached! Initiating All-Out Attack!</color>");
            ChangeState(CommanderState.AllOutAttack);
            return;
        }

        if (currentState == CommanderState.Scavenging && AreResourcesDepleted())
        {
            Debug.Log("<color=red>Enemy Commander: Resources depleted! Forcing All-Out Attack!</color>");
            ChangeState(CommanderState.AllOutAttack);
            return;
        }
    }

    public void TriggerDefenseMode()
    {
        if (currentState == CommanderState.AllOutAttack) return; 
        
        Debug.Log("<color=yellow>Enemy Commander: Base under attack! Recalling squads to defend!</color>");
        ChangeState(CommanderState.Defending);
    }

    private void ChangeState(CommanderState newState)
    {
        currentState = newState;
        IssueGlobalCommand();
    }

    private void IssueGlobalCommand()
    {
        foreach (LeadNav squad in activeEnemySquads)
        {
            if (squad == null) continue;

            if (currentState == CommanderState.AllOutAttack)
            {
                squad.target = playerBase; 
                squad.task = AntTask.Manual; 
            }
            else if (currentState == CommanderState.Defending)
            {
                squad.target = enemyBase;
                squad.task = AntTask.Manual; 
            }
        }
    }

    private void HandleScavenging()
    {
        foreach (LeadNav squad in activeEnemySquads)
        {
            if (squad != null && squad.task != AntTask.Food)
            {
                squad.task = AntTask.Food; 
                squad.home = enemyBase;    
            }
        }
    }

    private void HandleDefending()
    {
        foreach (LeadNav squad in activeEnemySquads)
        {
            if (squad != null && squad.target != enemyBase)
            {
                squad.target = enemyBase; 
            }
        }
    }

    private void HandleAllOutAttack()
    {
        foreach (LeadNav squad in activeEnemySquads)
        {
            if (squad != null && squad.target != playerBase)
            {
                squad.target = playerBase;
            }
        }
    }

    private bool AreResourcesDepleted()
    {
        GameObject[] foodItems = GameObject.FindGameObjectsWithTag("Food");
        return foodItems.Length == 0;
    }

    private void ApplyPersonalityTraits()
    {
        switch (personality)
        {
            case EnemyPersonality.Aggressive:
                armySizeThreshold = 5; 
                timeUntilAttack = 300f; // 5 mins
                break;
            case EnemyPersonality.Turtling:
                armySizeThreshold = 20; 
                timeUntilAttack = 900f; // 15 mins
                break;
            case EnemyPersonality.Balanced:
            default:
                armySizeThreshold = 10;
                timeUntilAttack = 600f; // 10 mins
                break;
        }
    }

    public void RegisterNewSquad(LeadNav newSquad)
    {
        AntBrain brain = newSquad.GetComponent<AntBrain>();
    
        if (brain != null && brain.antType.teamID == 1) 
        {
            if (!activeEnemySquads.Contains(newSquad))
            {
                activeEnemySquads.Add(newSquad);
                IssueGlobalCommand(); 
            }
        }
    }
}