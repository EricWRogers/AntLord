using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;
    public int food = 1000;
    public int sand = 0;

    void Start()
    {
        if (instance == null || instance != this)
        {
            instance = this;
        }
    }


    public int GetFood()
    {
        return food;
    }

    public int GetSand()
    {
        return sand;
    }
    public void AddFood(int amount)
    {
        food += amount;
        //Debug.Log(food);
    }

    public void AddSand(int amount)
    {
        sand += amount;
    }

    public void SubSand(int amount)
    {
        if (sand > 0)
        {
           sand -= amount; 
        }
    }
}
