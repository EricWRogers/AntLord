using UnityEngine;

public class RaycastInteraction : MonoBehaviour
{
   public LineRenderer lineRenderer;
   public LayerMask uiLayer;
   
   void Update() 
   {
       Ray ray = new Ray(transform.position, transform.forward);
       if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, uiLayer)) {
           lineRenderer.SetPosition(1, hit.point);
           // Handle button interaction logic here
       } 
       else 
       {
           lineRenderer.SetPosition(1, transform.position + transform.forward * 10);
       }
   }
}