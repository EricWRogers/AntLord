using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : MonoBehaviour
{
    private Transform target;
    public int damgeAmount = 25;
    public timer destroyTimer;
    public float duration = 5f;
    LayerMask layermask;

    void Awake()
    {
        layermask = LayerMask.GetMask("Terrain");
    }

    private void Start()
    {
        destroyTimer = gameObject.AddComponent<timer>();
        if (destroyTimer.timeout == null)
            destroyTimer.timeout = new UnityEvent();

        destroyTimer.timeSet = duration;
        destroyTimer.timeout.AddListener(End);
    }
    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 0.06f, layermask))
        {
            Destroy(gameObject);
        }
        destroyTimer.StartTime();
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
    public void End()
    {
        Destroy(gameObject);
    }
}
