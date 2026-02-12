using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpawnerBuilding : Buildings
{

    [SerializeField] BuildingSO spawnerSO;
    public Transform spawnPoint;
    public float spawnPadding = 5.0f;
    float timer = 0.0f;
    public float spawnCooldown = 1.0f;
    public int maxAnts = 10;
    List<GameObject> ants = new List<GameObject>();


    void Start()
    {
        this.currentHealth = spawnerSO.buildHealth;
        slider.maxValue = currentHealth;
        slider.value = currentHealth;
    }
    void FixedUpdate()
    {
        if (ants.Count < maxAnts && this.currentHealth > 0)
        {
            timer += Time.deltaTime;
            if (timer >= spawnCooldown)
            {
                SpawnAnt();
                timer = 0;
            }
        }
    }

    void SpawnAnt()
    {
        this.TakeDamage(3);
        Instantiate(spawnerSO.ant, spawnPoint.position, Quaternion.identity); //idk what to do about rotation at the moment so...
    }
}
