using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform target;
    public int damgeAmount = 25;
    LayerMask layermask;

    void Awake()
    {
        layermask = LayerMask.GetMask("Terrain");
    }
    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 0.04f,layermask ))
        {
            Destroy(gameObject);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ant"))
        {
            AntBrain ant = other.GetComponent<AntBrain>();
            ant.TakeDamage(damgeAmount);
            Destroy(gameObject);
        }
        
    }
}
