using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class WinLoseCondition : MonoBehaviour
{   
    public static WinLoseCondition instance;

    Buildings[] _buildings;
    public  List<Buildings> _playerBuildings = new List<Buildings>();
    public List<Buildings> _enemyBuildings = new List<Buildings>();
    AntSelectGlow[] _playerAnts;

    public GameObject nextLevelBttn;
    public GameObject retryBttn;
    public GameObject mmBttn;

    [Tooltip("Temporary booleans for testing only")]
    public bool triggerWin;
    public bool triggerLoss;

    void Awake()
    {
        gameObject.SetActive(true);
    }

    void Start()
    {
        if (instance == null || instance != this)
        {
            instance = this;
        }

        //disable UI
        nextLevelBttn.SetActive(false);
        retryBttn.SetActive(false);
        mmBttn.SetActive(false);

        //get list of all bases
        _buildings = FindObjectsByType<Buildings>(FindObjectsSortMode.None);

        foreach (Buildings building in _buildings)
        {
            if (building.teamID == 0)
            {
                _playerBuildings.Add(building);
            }
            else if (building.teamID == 1)
            {
                _enemyBuildings.Add(building); 
            }
        }
        Debug.Log("Number of buildings owned by player: " + _playerBuildings.Count);
        Debug.Log("Number of buildings owned by enemy ai: " + _enemyBuildings.Count);
    }

    // Update is called once per frame
    void Update()
    {
        //get list of all player ants
        _playerAnts = FindObjectsByType<AntSelectGlow>(FindObjectsSortMode.None);

        if (_playerAnts.Length == 0 || _playerBuildings.Count == 0 || (!triggerWin && triggerLoss))
        {
            //lose!
            Debug.Log("Lose game!");

            //play cutscene...

            nextLevelBttn.SetActive(false);
            retryBttn.SetActive(true);
            mmBttn.SetActive(true);
        }

        if (_enemyBuildings.Count == 0 || (triggerWin && !triggerLoss))
        {
            //win!
            Debug.Log("Win game!");
            
            //play cutscene...
            
            nextLevelBttn.SetActive(true);
            retryBttn.SetActive(true);
            mmBttn.SetActive(true);
        }
    }

    public void OnPlayerBuildingCreated(GameObject buildingPrefab)
    {
        Buildings building = buildingPrefab.GetComponent<Buildings>();
        _playerBuildings.Add(building);
    }

    public void OnPlayerBuildingDestroyed(GameObject buildingPrefab)
    {
        Buildings building = buildingPrefab.GetComponent<Buildings>();
        _playerBuildings.Remove(building);
    }

    public void OnEnemyBuildingDestroyed(GameObject buildingPrefab)
    {
        Buildings building = buildingPrefab.GetComponent<Buildings>();
        _enemyBuildings.Remove(building);
    }

    public void NextLevel(string lvlName)
    {
        SceneManager.LoadScene(lvlName);
    }

    public void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturntoMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
