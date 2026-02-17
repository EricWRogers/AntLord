using UnityEngine;

public class FoodBites : MonoBehaviour
{
    public GameObject foodBite;
    public GameObject ant;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    // void Update()
    // {
    //     //foodBite.transform.parent = ant.transform;
    //     foodBite.transform.SetParent(ant.transform);
    // }
    public void SetAnt(GameObject item)
    {
        ant = item;
        foodBite.transform.SetParent(ant.transform);
    }
}
