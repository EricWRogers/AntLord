using UnityEngine;
using Debug = UnityEngine.Debug;

public class TestBuilding : MonoBehaviour
{
    //Reminder for myself because I will forget in 3 hours:
    //Im thinking of making all buildings inherit from a Building class thats a Monobehaviour
    //the Building class is what will hold all the code to place and maybe buy buildings
    //Anyway all buildings would inherit from it and then I split them into their types
    //(spawining, resources, storage, etc.) and it would be very similar to this kind of script
    //I cant wait for this to become an unmanageable mess. 
    //If all this building code is still readable by March, Im making an upgrade tree for each building and thats a threat


    [SerializeField] BuildingSO testSO;
    public Transform spawnPoint;
    public float spawnPadding = 5.0f;
    float timer = 0.0f;
    public float spawnCooldown = 1.0f;
    public int currentHealth;

    void Start()
    {
        currentHealth = testSO.buildHealth;
        Debug.Log(testSO.buildName);
    } 
    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if(timer >= spawnCooldown)
        {
            SpawnAnt();
            //currentHealth -= 10;
            timer = 0;
        }
        if(currentHealth <= 0)
        {
            Debug.Log("building destroyed");
            Destroy(gameObject);
        }
    }

    void SpawnAnt()
    {
        Instantiate(testSO.ant, spawnPoint.position, Quaternion.identity); //idk what to do about rotation at the moment so...
    }
}
