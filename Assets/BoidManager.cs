using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [Header("General Boid Settings")]
    public GameObject boidModel;
    public int numBoids = 50;
    [Range(1.0f, 10.0f)]
    public float neighbourDistance = 3;

    [Header("Target Settings")]
    public Vector3 limits = new Vector3(5.0f, 5.0f, 5.0f);
    public Vector3 goal = Vector3.zero;

    [Header("Weight Settings")]
    [Range(0.0f, 20.0f)]
    public float w_alignment = 15f;
    [Range(0.0f, 20.0f)]
    public float w_avoidance = 2.5f;
    [Range(0.0f, 20.0f)]
    public float w_flatten = 5f;
    [Range(0.0f, 20.0f)]
    public float w_center = 2.5f;
    [Range(0.0f, 20.0f)]
    public float w_global_center = 1f;
    [Range(0.0f, 1.0f)]
    public float w_random_movement = 0.1f;

    [Header("Speed Settings")]
    [Range(0.0f, 10.0f)]
    public float minSpeed = 1.0f;
    [Range(0.0f, 10.0f)]
    public float maxSpeed = 5.0f;
    [Range(0.0f, 5.0f)]
    public float rotationSpeed = 1.76f;

    public List<Boid> boids;    // List of active boids
    public Vector3 center;      // Center of the boids

    // Start is called before the first frame update
    void Start()
    {
        boids = new List<Boid>();

        // Instantiate all of the new boids
        for (int i = 0; i < numBoids; ++i)
        {
            // Position
            Vector3 randPos = new Vector3(Random.Range(-1 * limits.x, limits.x), Random.Range(-1 * limits.y, limits.y), Random.Range(-1 * limits.z, limits.z));

            // New Object
            var boid_go = Instantiate(boidModel, boidModel.transform.position + randPos, boidModel.transform.rotation);
            var boid = boid_go.GetComponent<Boid>();

            boid.boidManager = this;
            boid.ID = i;

            // Add to the list
            boids.Add(boid);

            // Randomly set the goal
            goal = new Vector3(Random.Range(-1 * limits.x, limits.x), Random.Range(-1 * limits.y, limits.y), Random.Range(-1 * limits.z, limits.z));
        }

        // Update the center
        center = CalcCenter();
    }

    // Update is called once per frame
    void Update()
    {
        // Randomly set the goal
        if (Random.Range(0, 500) < 1)
            goal = new Vector3(Random.Range(-1 * limits.x, limits.x), Random.Range(-1 * limits.y, limits.y), Random.Range(-1 * limits.z, limits.z));

        // Update the center
        center = CalcCenter();
    }

    // Destroy all current boids and call start
    public void Restart()
    {
        for (int i = 0; i < boids.Count; ++i)
        {
            Destroy(boids[i].gameObject);
        }

        Start();
    }

    // Find the center of the boids
    public Vector3 CalcCenter()
    {
        var sum_pos = Vector3.zero;

        foreach (var boid in boids)
        {
            sum_pos += boid.transform.position;
        }

        return sum_pos / boids.Count;
    }

    // Set the alignment weight
    public void UpdateAlignment(float new_alignment)
    {
        w_alignment = new_alignment;
    }

    // Set the avoidance weight
    public void UpdateAvoidance(float new_avoidance)
    {
        w_avoidance = new_avoidance;
    }

    // Set the flatten weight
    public void UpdateFlatten(float new_flatten)
    {
        w_flatten = new_flatten;
    }

    // Set the center weight
    public void UpdateCenter(float new_center)
    {
        w_center = new_center;
    }

    // Set the global center weight
    public void UpdateGlobalCenter(float new_global_center)
    {
        w_global_center = new_global_center;
    }

    // Set the random weight
    public void UpdateRandom(float new_random)
    {
        w_random_movement = new_random;
    }

    // Set the min speed
    public void UpdateMinSpeed(float new_min_speed)
    {
        minSpeed = new_min_speed;
    }

    // Set the max speed
    public void UpdateMaxSpeed(float new_max_speed)
    {
        maxSpeed = new_max_speed;
    }

    // Set the rotation speed
    public void UpdateRotationSpeed(float new_rotation_speed)
    {
        rotationSpeed = new_rotation_speed;
    }
}
