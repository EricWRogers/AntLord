using UnityEngine;


public class VRConflictPrevention : MonoBehaviour
{
    public GameObject VRPLayer;
    public GameObject DesktopCam;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        if (VRPLayer.activeInHierarchy || DesktopCam.activeInHierarchy)
        {
            this.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
