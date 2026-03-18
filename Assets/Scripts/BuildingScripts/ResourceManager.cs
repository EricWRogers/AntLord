using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;
    public int food = 1000;

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
    public void AddFood(int amount)
    {
        food += amount;
        //Debug.Log(food);
    }
}
