using UnityEngine;


public class VRConflictPrevention : MonoBehaviour
{
    public GameObject VRPLayer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        if (VRPLayer.activeInHierarchy)
        {
            this.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
