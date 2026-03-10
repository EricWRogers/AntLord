using UnityEngine;

public class FoodBites : MonoBehaviour
{
    public GameObject foodBite;
    public GameObject ant;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (transform.parent == null)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ant.GetComponent<LeadNav>().amCarryingFood == false && !ant.GetComponent<FollowNav>().enabled)
        {
            Destroy(gameObject);
            Debug.Log("destoryed food");
        }
        else if (ant.GetComponent<FollowNav>().amCarryingFood == false && !ant.GetComponent<LeadNav>().enabled)
        {
            Destroy(gameObject);
            Debug.Log("destoryed food");
        } 
    }
    public void SetAnt(GameObject item)
    {
        foodBite.transform.SetParent(item.transform);
        ant = item;
    }
}
