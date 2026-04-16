using UnityEngine;

public class EnemyBuildingManager : MonoBehaviour
{
    [Header("Building Settings")]
    public GameObject buildingPrefab;
    public int foodRequiredPerBuilding = 5;
    private int currentFoodCount = 0;

    [Header("Spawn Layout")]
    public float spawnRadius = 10f;

    public void AddFood()
    {
        currentFoodCount++;
        Debug.Log($"Enemy Base Food: {currentFoodCount}/{foodRequiredPerBuilding}");

        if (currentFoodCount >= foodRequiredPerBuilding)
        {
            SpawnNewBuilding();
            currentFoodCount = 0;
        }
    }

    private void SpawnNewBuilding()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        Instantiate(buildingPrefab, spawnPos, Quaternion.identity);
        Debug.Log("<color=red>Enemy Commander: New building constructed!</color>");
    }
}
