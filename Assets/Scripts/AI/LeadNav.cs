using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class LeadNav : MonoBehaviour
{
    private NavMeshAgent myAgent;
    public List<NavMeshAgent> followers;
    public GameObject crumbPrefab;
    public List<Vector3> crumbs;
    public float crumbDropDelay = 1f;
    private float crumbDropTimer = 0f;
    public Transform target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!myAgent.isStopped){
            crumbDropTimer += Time.deltaTime;

            if(crumbDropTimer >= crumbDropDelay)
            {
                crumbDropTimer = 0f;
                
                //Debug crumb
                crumbs.Add(Instantiate(crumbPrefab, transform.position, Quaternion.identity).transform.position);
                
                //invisible crumb
                //crumbs.Add(transform.position);
            }

            myAgent.transform.LookAt(target.position);

            myAgent.destination = target.position;
        }
    }

    void FixedUpdate()
    {
        // Using distance calculation constantly is bad for performance, need alternate method
        if(!myAgent.isStopped && Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            Debug.Log("Hit target");
            myAgent.isStopped = true;
            crumbDropTimer = 0f;
        }
    }

    // never got called?
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Finish")
        {
            Debug.Log("Hit target");
            myAgent.isStopped = true;
            crumbDropTimer = 0f;
        }
    }
}
