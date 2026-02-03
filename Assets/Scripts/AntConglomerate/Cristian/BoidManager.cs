using UnityEngine;
using System.Collections.Generic;

public class BoidManager : MonoBehaviour
{
    public Boid boidPrefab;
    public int boidCount = 50;
    public Vector3 spawnBounds = new Vector3(20, 20, 20);

    [HideInInspector]
    public List<Boid> boids = new List<Boid>();

    void Start()
    {
        for (int i = 0; i < boidCount; i++)
        {
            Vector3 position = transform.position + new Vector3(
                Random.Range(-spawnBounds.x, spawnBounds.x),
                Random.Range(-spawnBounds.y, spawnBounds.y),
                Random.Range(-spawnBounds.z, spawnBounds.z)
            );

            Boid newBoid = Instantiate(boidPrefab, position, Quaternion.identity);
            boids.Add(newBoid);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, spawnBounds * 2);
    }
}

//actually implement fully into game

//determine if we will manage by invisible checkpoints that enemy or follow able enemies drop and also check my main pc for the extra info like about theramone pathing.  Having a time limit for how long it lasts and which one is more important to detect

//Erics Stolen Stuff Below
/*
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.TextCore;

public class BoidManager : MonoBehaviour
{
    [Header("Professors Stuff")]
    public List<Transform> rocks;

    public float maxAlignmentDistance = 3.0f;
    public float maxCohesionDistance = 2.0f;
    public float maxSeparationDistance = 0.5f;
    public float maxBombDistance = 1.0f;
    public float fleeWeight = 1.0f;
    public float targetWeight = 1.0f;
    public float separationWeight = 1.0f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;
    public float drag = 0.95f;
    public float speed = 1.0f;
    public float maxSpeed = 3.0f;

    [Header("Me Stuff")]
    public List<Transform> checkPoints;          // list of checkpoint targets
    private int currentPointIndex = 0;           // current checkpoint index
    public float wayPointRange = 5.0f;           // distance boids must be within to count asat a waypoint
    public float groupReachRatio = 0.5f;         // percent of boids needed to move to next waypoint
    //public float waitTimeAtTarget = 0.5f;      
    //private float waitTimer = 0f;


    //READ THIS FIRST: This script effectively has a checkpoint system that is tracked using gameobjects that also have questionable wait timers implemented that help with the group ratio statement in order to do the 10 percent part or atleast thats my method for it to recognize if all of the boids are there, simply it tracks if the input percentage is in the waypoints radius and if it is YIPPEEEE they go to the next one, ignore how terrifyingly jacked up this looks :]

    // Start is called before the first frame update
    void Start()
    {
        // grab all "ROCK" tagged objects
        GameObject[] gos = GameObject.FindGameObjectsWithTag("ROCK");
        rocks.Clear();
        foreach (GameObject go in gos)
        {
            rocks.Add(go.transform);
        }

        // grab all "CHECKPOINT" tagged objects
        GameObject[] cps = GameObject.FindGameObjectsWithTag("CHECKPOINT");
        checkPoints.Clear();
        foreach (GameObject c in cps)
        {
            checkPoints.Add(c.transform);
        }
    }

    void Update()
    {
        if (checkPoints.Count == 0) return;

        Vector2 targetPos = checkPoints[currentPointIndex].position;
        Boid[] boids = GetComponentsInChildren<Boid>();

        int nearBoids = 0; // count how many are near the current checkpoint

        foreach (Boid boid in boids)
        {
            Vector2 pos = boid.transform.position;

            Vector2 fleeDirection = Flee(pos);
            Vector2 seekDirection = Seek(pos, targetPos);
            Vector2 separationDirection = Separation(boid, pos);
            Vector2 alignmentDirection = Alignment(boid, pos);
            Vector2 cohesionDirection = Cohesion(boid, pos);

            // combined steering stuff with other stuff above that do the stuff, my comments earlier exploded
            boid.acceleration = (seekDirection * targetWeight) +
                                (separationDirection * separationWeight) +
                                (alignmentDirection * alignmentWeight) +
                                (cohesionDirection * cohesionWeight) +
                                (fleeDirection * fleeWeight);

            //LEGACY M O T I O N idk just typing atp
            boid.velocity += boid.acceleration * speed;
            boid.velocity *= drag;

            // same max speed limter
            if (boid.velocity.magnitude > maxSpeed)
                boid.velocity = boid.velocity.normalized * maxSpeed;

        
            pos += boid.velocity * Time.deltaTime;
            boid.transform.position = new Vector3(pos.x, pos.y, boid.transform.position.z);

            // rotate to face direction of travel
            if (boid.velocity.sqrMagnitude > 0.0001f)
                boid.transform.right = boid.velocity.normalized;

            // count boids close to checkpoint
            if (Vector2.Distance(pos, targetPos) <= wayPointRange)
                nearBoids++;
        }

        float groupRatio = (float)nearBoids / boids.Length;
        if (groupRatio >= groupReachRatio)
        {
            // choose a boid that’s close to the target (acts as leader) and I am aware that this is scuffed but I am tired now, it works most of the time
            Boid leader = boids[0];
            float closestDist = float.MaxValue;
            foreach (Boid b in boids)
            {
                float dist = Vector2.Distance(b.transform.position, checkPoints[currentPointIndex].position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    leader = b;
                }
            }

            // let that leader notify neighbors
            NotifyNeighborsOfTargetChange(leader, boids);

            // now switches the main group target
            currentPointIndex++;
            if (currentPointIndex >= checkPoints.Count)
                currentPointIndex = 0;
        }
    }

    // Makes nearby boids switch to the next checkpoint when one reaches it
    void NotifyNeighborsOfTargetChange(Boid arrivedBoid, Boid[] allBoids)
    {
        // Which checkpoint this boid just reached
        int targetToSwitchFrom = currentPointIndex;

        foreach (Boid neighborBoid in allBoids)
        {
            if (neighborBoid == arrivedBoid) continue;

            // if another boid is close enough and still aiming for the same target
            float distance = Vector2.Distance(arrivedBoid.transform.position, neighborBoid.transform.position);
            if (distance < maxAlignmentDistance)
            {
                // nudge them toward the next checkpoint
                Vector2 nextTargetPos = checkPoints[(targetToSwitchFrom + 1) % checkPoints.Count].position;
                Vector2 direction = (nextTargetPos - (Vector2)neighborBoid.transform.position).normalized;
                neighborBoid.acceleration += direction * targetWeight;
            }
        }
    }




    // make boids flee away from nearby rocks
    Vector2 Flee(Vector2 _agentPos)
    {
        Vector2 flee = Vector2.zero;

        foreach (Transform bomb in rocks)
        {
            Vector2 bombPos = new Vector2(bomb.position.x, bomb.position.y);
            float distance = Vector2.Distance(_agentPos, bombPos);
            if (distance < maxBombDistance)
            {
                Vector2 direction = _agentPos - bombPos;
                flee += direction;
            }
        }

        if (flee != Vector2.zero)
            flee = flee.normalized;

        return flee;
    }

    // seek a target position (e.g., checkpoint or mouse)
    Vector2 Seek(Vector2 _agentPos, Vector2 _targetPos)
    {
        Vector2 seek = _targetPos - _agentPos;
        return seek.normalized;
    }

    // keep boids apart from each other
    Vector2 Separation(Boid _boid, Vector2 _agentPos)
    {
        Vector2 separation = Vector2.zero;
        Boid[] boids = GetComponentsInChildren<Boid>();

        foreach (Boid neighborBoid in boids)
        {
            if (_boid.gameObject != neighborBoid.gameObject)
            {
                Vector2 neighborPos = new Vector2(neighborBoid.transform.position.x, neighborBoid.transform.position.y);
                float distance = Vector2.Distance(_agentPos, neighborPos);
                if (distance < maxSeparationDistance)
                {
                    // linear falloff to separation
                    separation += (_agentPos - neighborPos).normalized * (maxSeparationDistance - distance);
                }
            }
        }

        if (separation != Vector2.zero)
            separation.Normalize();

        return separation;
    }

    // align boids' heading with neighbors
    Vector2 Alignment(Boid _boid, Vector2 _agentPos)
    {
        Vector2 alignment = Vector2.zero;
        int numberOfNeighbors = 0;
        Boid[] boids = GetComponentsInChildren<Boid>();

        foreach (Boid neighborBoid in boids)
        {
            if (_boid.gameObject != neighborBoid.gameObject)
            {
                Vector2 neighborPos = new Vector2(neighborBoid.transform.position.x, neighborBoid.transform.position.y);
                float distance = Vector2.Distance(_agentPos, neighborPos);
                if (distance < maxAlignmentDistance)
                {
                    numberOfNeighbors++;
                    alignment += neighborBoid.velocity;
                }
            }
        }

        if (numberOfNeighbors > 0)
            return (alignment / (float)numberOfNeighbors).normalized;

        return Vector2.zero;
    }

    // keep boids moving toward center of nearby group
    Vector2 Cohesion(Boid _boid, Vector2 _agentPos)
    {
        Vector2 cohesion = Vector2.zero;
        int numberOfNeighbors = 0;
        Boid[] boids = GetComponentsInChildren<Boid>();

        foreach (Boid neighborBoid in boids)
        {
            Vector2 neighborPos = new Vector2(neighborBoid.transform.position.x, neighborBoid.transform.position.y);
            float distance = Vector2.Distance(_agentPos, neighborPos);

            if (distance < maxCohesionDistance)
            {
                cohesion += neighborPos;
                numberOfNeighbors++;
            }
        }

        if (numberOfNeighbors > 0)
        {
            // get average neighbor position
            cohesion /= (float)numberOfNeighbors;
            cohesion -= _agentPos;

            if (cohesion != Vector2.zero)
                return cohesion.normalized;
        }

        return Vector2.zero;
    }

    // visualize waypoints and current target in scene view
    void OnDrawGizmos()
    {
        if (checkPoints == null || checkPoints.Count == 0) return;

        for (int i = 0; i < checkPoints.Count; i++)
        {
            Gizmos.color = (i == currentPointIndex) ? Color.green : Color.yellow;
            Gizmos.DrawSphere(checkPoints[i].position, 0.25f);

            int next = (i + 1) % checkPoints.Count;
            Gizmos.DrawLine(checkPoints[i].position, checkPoints[next].position);
        }
    }
}*/
