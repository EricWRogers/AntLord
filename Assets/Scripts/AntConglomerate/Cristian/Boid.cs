using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Boid : MonoBehaviour
{
    public float speed = 5f;
    public float neighborDistance = 3f;
    public float separationDistance = 1f;
    public float rotationSpeed = 2f;

    private Vector3 velocity;
    private BoidManager manager;

    void Start()
    {
        manager = FindObjectOfType<BoidManager>();
        velocity = transform.forward * speed;
    }

    void Update()
    {
        if (manager == null) return;

        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;

        int neighborCount = 0;

        foreach (Boid other in manager.boids)
        {
            if (other == this) continue;

            float distance = Vector3.Distance(transform.position, other.transform.position);

            if (distance <= neighborDistance)
            {
                // Separation
                if (distance < separationDistance)
                    separation += (transform.position - other.transform.position).normalized / distance;

                // Alignment
                alignment += other.velocity;

                // Cohesion
                cohesion += other.transform.position;

                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            alignment /= neighborCount;
            cohesion = (cohesion / neighborCount - transform.position).normalized;
        }

        // Combine forces
        Vector3 direction = separation + alignment + cohesion;
        if (direction != Vector3.zero)
        {
            velocity = Vector3.Lerp(velocity, direction.normalized * speed, Time.deltaTime * rotationSpeed);
        }

        transform.position += velocity * Time.deltaTime;
        transform.forward = velocity.normalized;
    }

    public Vector3 Velocity => velocity;
}

//Erics Stolen Stuff Below

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour //can modify this to where each boid has its own target
{
    public Vector2 velocity;
    public Vector2 acceleration;
    public float speed = 0.0f;
}
*/