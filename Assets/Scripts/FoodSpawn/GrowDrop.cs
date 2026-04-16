using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GrowDrop : MonoBehaviour
{

    //size & growth during/after spawn 
    // public SBBerrySpawn sBBS;
    public float gdGrowDuration;
    public float startingSize = 0.1f;
    Coroutine growingApple;
    private bool isGrowing = false;

    //gravity
    private Rigidbody rb;
    float timer;
    
    void Start()
    {
        StartCoroutine(StartGrowing());
    }


    IEnumerator StartGrowing()
    {
        Rigidbody rb = this.GetComponent<Rigidbody>();
            // Debug.Log("rb: "+rb);

            if (rb != null && rb.useGravity == true)
            {
                rb.useGravity = false;
                // Debug.Log ("Turned off to grow");
            }

        this.transform.localScale = new Vector3 (startingSize, startingSize, startingSize);

        growingApple = StartCoroutine(WatchAppleGrow(this.transform));
    
        CollisionDetectorForPrefabs forwarder = this.GetComponent<CollisionDetectorForPrefabs>();
        forwarder.OnHit = (collision) =>
        {
            Debug.Log("The berry hit: " + collision.gameObject.name);
            
            if (isGrowing == true)
            {
                StopCoroutine(growingApple);    
                isGrowing = false;
                Debug.Log ("stopped coroutine grow");
            }
            

            if (rb != null && rb.useGravity != true)
            {
                rb.useGravity = true;
                Debug.Log ("started gravity early");
            }
        };
        
        yield return new WaitForSeconds(gdGrowDuration);

        if (rb != null)
        {
        rb.useGravity = true;
            Debug.Log ("rb");
        }
        else
        {
        Debug.Log ("No rigidbody");
        }
    }



        IEnumerator WatchAppleGrow(Transform objTransform)
    {
        Debug.Log("IEnum");
        isGrowing = true;
        float elapsed = 0f;
        Vector3 startScale = new Vector3 (startingSize, startingSize, startingSize);
        Vector3 endScale = Vector3.one;

        while (elapsed < gdGrowDuration)
        {
            Debug.Log("elapsed < gdGrowDuration");
            elapsed += Time.deltaTime;
            objTransform.localScale = Vector3.Lerp(startScale, endScale, elapsed / gdGrowDuration);
            yield return null;
        }
        objTransform.localScale = endScale;
        isGrowing = false;
    }
}
