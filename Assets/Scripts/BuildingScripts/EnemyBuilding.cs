using UnityEngine;
using System.Collections.Generic;

public class EnemyBuilding : Buildings
{
    [SerializeField] BuildingSO spawnerSO;
    public Transform spawnPoint;
    public float spawnPadding = 5.0f;
    float timer = 0.0f;
    public float spawnCooldown = 8.0f;
    public int maxAnts = 10;
    List<GameObject> ants = new List<GameObject>();
    public GameObject antPrefab;
    public int maxHealth = 10;

    void Start()
    {
        this.currentHealth = maxHealth;
        slider.maxValue = currentHealth;
        slider.value = currentHealth;
    }
    void FixedUpdate()
    {
        if (ants.Count < maxAnts && this.currentHealth > 0) //GameManager.instance.GetFood() > minFoodPerAnt
        {
            timer += Time.deltaTime;
            if (timer >= spawnCooldown)
            {
                SpawnAnt();
                timer = 0;
            }
        }
        if (this.currentHealth <= 0)
        {
            FindFirstObjectByType<MM>().Pause();
        }
    }

    void SpawnAnt()
    {
        Instantiate(antPrefab, spawnPoint.position, Quaternion.identity); //idk what to do about rotation at the moment so...
        
    }
}
