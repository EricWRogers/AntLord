using UnityEngine;
using UnityEngine.SceneManagement;

public class MM : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play(string level)
    {
        SceneManager.LoadScene(level);
        Debug.Log("Game Started.");
    }

    public void Options(GameObject menu)
    {
        menu.SetActive(true);
        Debug.Log("Options Menu Opened.");
    }

    public void Quit()
    {
        Debug.Log("Bye, Bye!");
        Application.Quit();
    }
}
