using UnityEngine;

public class FoodBites : MonoBehaviour
{
    public GameObject foodBite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foodBite.transform.position = new Vector3(foodBite.transform.position.x, 1f, foodBite.transform.localPosition.z);
    }

    // Update is called once per frame
    // void Update()
    // {
    //     //foodBite.transform.parent = ant.transform;
    //     foodBite.transform.SetParent(ant.transform);
    // }
    public void SetAnt(GameObject item)
    {
        foodBite.transform.SetParent(item.transform);
    }
}
