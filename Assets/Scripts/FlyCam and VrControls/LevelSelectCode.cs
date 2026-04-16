using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectCode : MonoBehaviour
{
    public GameObject levelSelectMenu;

    void Start()
    {
        levelSelectMenu.SetActive(false);
    }

     public void StartLevel1(string level){
        SceneManager.LoadScene(level);
        Debug.Log("Game Started: " + level);
    }
    public void StartLevel2(string level){
        SceneManager.LoadScene(level);
        Debug.Log("Game Started level 2: " + level);
    }
    public void StartLevel3(string level){
        SceneManager.LoadScene(level);
        Debug.Log("Game Started level 3: " + level);
    }

    public void BackButton(){ //hide level select
        levelSelectMenu.SetActive(false);
        
    }

    public void ShowLevelMenu(){ //show level select
        levelSelectMenu.SetActive(true);
        
    }
}
