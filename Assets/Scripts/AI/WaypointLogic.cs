using UnityEngine;

public class WaypointLogic : MonoBehaviour
{
    Collider[] hitColliders;
    public float radius = 1f;
    public float rotSpeed = 0.1f;
    public Transform baseTransform;


    // Update is called once per frame
    void Update()
    {
        transform.localRotation = new Quaternion(transform.localRotation.x, transform.localRotation.y + rotSpeed * Time.deltaTime, transform.localRotation.z, transform.localRotation.w);

        hitColliders = Physics.OverlapSphere(baseTransform.position, radius);

        foreach (var hitColliders in hitColliders)
        {
            if(hitColliders.CompareTag("Ant"))
            {
                gameObject.SetActive(false);
            }
        }
    }
}
