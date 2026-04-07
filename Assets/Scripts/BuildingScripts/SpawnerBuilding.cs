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
    public int tempWinCon;
    public float radius = 5.0f;
    List<GameObject> ants = new List<GameObject>();
    public GameObject antPrefab;
    public int maxHealth = 10;
    Bounds bounds;

    public GameObject LoseScreen;


    void Start()
    {
        this.currentHealth = maxHealth;
        slider.maxValue = currentHealth;
        slider.value = currentHealth;
        bounds = GetComponent<MeshRenderer>().bounds;

        LoseScreen.SetActive(false);
    }
    void FixedUpdate()
    {
        if (ants.Count < maxAnts && ResourceManager.instance.GetFood() > minFoodPerAnt && this.currentHealth > 0)
        {
            timer += Time.deltaTime;
            if (timer >= spawnCooldown)
            {
                SpawnAnt();
                timer = 0;
            }
            if (this.currentHealth <= 0)
            {
                FindFirstObjectByType<MM>().Pause();
                LoseScreen.SetActive(true);
            }
        }
    }

    void SpawnAnt()
    {
        //Vector3 padding = new Vector3(Random.Range(0.0f, spawnPadding), gameObject.transform.position.y + 1.0f, Random.Range(0.0f, spawnPadding));
        ResourceManager.instance.AddFood(-2);
        float randomX = 0.0f;
        float randomZ = 0.0f;
        while (randomX < transform.localScale.x && randomX > -transform.localScale.x)
        {
            randomX = transform.position.x + transform.localScale.x + Random.Range(-(transform.localScale.x + radius), transform.localScale.x + radius);
        }
        while (randomZ < transform.localScale.z && randomZ > -transform.localScale.z)
        {
            randomZ = transform.position.z + transform.localScale.z + Random.Range(-(transform.localScale.z + radius), transform.localScale.z + radius);
        }

        Vector3 spawn = new Vector3(randomX, transform.position.y, randomZ);

        Instantiate(antPrefab, spawn, Quaternion.identity); //idk what to do about rotation at the moment so...
        tempWinCon++;
        if (tempWinCon >= 300)
        {
            FindFirstObjectByType<MM>().Pause();
        }
    }

    public void GiveFood(int _food) //or floats idk what yall are cooking
    {
        ResourceManager.instance.AddFood(_food);
    }
    public void GiveRock(int _rock) //or floats idk what yall are cooking
    {
        ResourceManager.instance.AddRock(_rock);
    }
    public void GiveStick(int _stick) //or floats idk what yall are cooking
    {
        Debug.Log("adding stick");
        ResourceManager.instance.AddStick(_stick);
    }
}
