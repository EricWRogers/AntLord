using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(RemoveFood))]
public class SpawnerBuilding : Buildings
{

    [SerializeField] BuildingSO spawnerSO;
    public Transform spawnPoint;
    public float spawnPadding = 5.0f;
    float timer = 0.0f;
    public float spawnCooldown = 5.0f;
    public int maxAnts = 10;
    public int foodAmount = 100; //placeholder for Im guessing will probably be stored in a GameManager
    public int minFoodPerAnt = 10;
    List<GameObject> ants = new List<GameObject>();


    void Start()
    {
        this.currentHealth = spawnerSO.buildHealth;
        slider.maxValue = currentHealth;
        slider.value = currentHealth;
    }
    void FixedUpdate()
    {
        if (ants.Count < maxAnts && foodAmount >= minFoodPerAnt && this.currentHealth > 0) //GameManager.instance.GetFood() > minFoodPerAnt
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
        //Vector3 padding = new Vector3(Random.Range(0.0f, spawnPadding), gameObject.transform.position.y + 1.0f, Random.Range(0.0f, spawnPadding));
        foodAmount -= 2; //10; //GameManager.instance.EatFood(10);
        Instantiate(spawnerSO.ant, spawnPoint.position, Quaternion.identity); //idk what to do about rotation at the moment so...
    }

    public void GiveFood(int _food) //or floats idk what yall are cooking
    {
        foodAmount += _food;
    }
}
