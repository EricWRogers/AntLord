using UnityEngine;

public class AntBrain : MonoBehaviour
{
    public AntDataType antType; // Drag your Worker_Data or Soldier_Data here in Inspector

    private float currentHealth;

    void Start()
    {
        // Initialize the ant based on its type
        currentHealth = antType.maxHealth;
        
        // If using NavMesh, set the speed
        GetComponent<UnityEngine.AI.NavMeshAgent>().speed = antType.moveSpeed;
    }

    public void Attack(AntBrain target)
    {
        // Use the damage stat from the ScriptableObject
        target.TakeDamage(antType.damage);
    }
}
