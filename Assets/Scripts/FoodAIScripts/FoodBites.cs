using UnityEngine;

public class FoodBites : MonoBehaviour
{
    public GameObject foodBite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (transform.parent == null)
        {
            Destroy(gameObject);
        }
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
