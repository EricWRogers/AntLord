using UnityEngine;

public class Turret : Buildings
{

    public Transform target;

    [Header("Attributes")]
    public float range = 10f;
    public float fireRate = 1f;
    public float bulletSpeed = 150f;
    public int maxHealth = 10;
    private float fireCountdown = 0f;
    [Header("Unity setup feilds")]
    public Transform partToRotate;
    public float turnSpeed = 5f;
    public GameObject bullet;
    public Transform firePoint;
    [SerializeField] BuildingSO turretSO;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
        this.currentHealth = maxHealth;
        slider.maxValue = currentHealth;
        slider.value = currentHealth;
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Ant");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach(GameObject enemy in enemies)
        {
            AntBrain enemyAnt = enemy.GetComponent<AntBrain>();
            if (enemyAnt.antType.teamID != teamID)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = enemy;
                    enemyAnt.currentTarget = null;
                    enemyAnt.currentBuildingTarget = GetComponent<Turret>();
                }
            }
        }

        if(nearestEnemy != null && shortestDistance <= range){
            target = nearestEnemy.transform;
        }else{
            target = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.currentHealth > 0)
        {
            if (this.currentHealth <= 0)
            {
                Destroy(gameObject);
            }
            if (target == null)
            {
                return;
            }

            Vector3 dir = target.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
            partToRotate.rotation = Quaternion.Euler(rotation.x, rotation.y, 0f);

            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = 1f / fireRate;
            }
            fireCountdown -= Time.deltaTime;
            
        }
        
    }

    void Shoot()
    {
        GameObject bulletGo = Instantiate(bullet, firePoint.position, firePoint.rotation);
        Rigidbody rb = bulletGo.GetComponent<Rigidbody>();
        Vector3 dir = target.position - transform.position;
        Vector3 dire = new Vector3(dir.x, (dir.y), dir.z);
        

        rb.AddForce(dire.normalized * bulletSpeed);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position,range);
    }
}
