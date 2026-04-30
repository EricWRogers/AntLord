using UnityEngine;

public class EnemyResourceManager : MonoBehaviour
{
    public static EnemyResourceManager instance;
    public int food = 0;
    public int sand = 0;
    public int rocks = 0;
    public int sticks = 0;

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
    public void AddRock(int amount)
    {
        rocks += amount;
        //Debug.Log(rocks);
    }
    public void AddStick(int amount)
    {
        sticks += amount;
        Debug.Log(sticks);
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
