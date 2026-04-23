using UnityEngine;
using TMPro;
using System.Linq;

public class StatsText : MonoBehaviour
{
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI sticksText;
    public TextMeshProUGUI stonesText;
    public TextMeshProUGUI antCountText;

    private SpawnFoodBites _spawnFoodBites;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //count all collected food


        //count all collected sticks


        //count all collected stones


        //count all of the players ants
        AntSelectGlow[] ants = FindObjectsByType<AntSelectGlow>(FindObjectsSortMode.None);
        antCountText.text = ("Total Ants: " + ants.Length.ToString());
        if (ants.Length == 0)
        {
            antCountText.color = new Color(212, 0, 20, 255);
        }
        else
        {
            antCountText.color = new Color(88, 20, 26, 255);
        }
    }
}
