using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    public Camera cam;
    public LayerMask placementMask; 
    public HeightmapTerrain hm;

    [Header("Placement")]
    public GameObject prefab;
    public float maxDistance = 300f;
    public bool alignToTerrainNormal = true;

    [Header("Physics")]
    public bool dropWithPhysics = true;
    public float settleTime = 0.25f; 
    public bool freezeAfterSettle = true;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (prefab == null || hm == null) return;

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0)) // place on left shift and click, change the key later
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, placementMask))
            {
                PlaceAt(hit.point);
            }
        }
    }

    void PlaceAt(Vector3 worldPoint)
    {
        // Snap to terrain surface height
        float y = hm.GetHeightWorldAtWorld(worldPoint);
        Vector3 pos = new Vector3(worldPoint.x, y, worldPoint.z);

        Quaternion rot = Quaternion.identity;

        if (alignToTerrainNormal)
        {
            
            Vector3 local = pos - hm.terrain.transform.position;
            float nx = Mathf.Clamp01(local.x / hm.data.size.x);
            float nz = Mathf.Clamp01(local.z / hm.data.size.z);
            Vector3 n = hm.data.GetInterpolatedNormal(nx, nz);

            
            rot = Quaternion.FromToRotation(Vector3.up, n);
        }

        GameObject obj = Instantiate(prefab, pos + Vector3.up * 0.15f, rot);

        
        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (dropWithPhysics)
        {
            if (rb == null) rb = obj.AddComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            if (freezeAfterSettle)
                StartCoroutine(FreezeLater(rb, settleTime));
        }
        else
        {
           
            if (rb != null) Destroy(rb);
        }
    }

    System.Collections.IEnumerator FreezeLater(Rigidbody rb, float t)
    {
        yield return new WaitForSeconds(t);
        if (rb == null) yield break;

        //tryt to prevent jitter
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
    }
}
