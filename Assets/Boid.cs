using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class Boid : MonoBehaviour
{
    public BoidManager boidManager; // Parent Manager
    public int ID = -1;             // ID from the Boid Manager
    
    private float speed;            // Translational Speed
    private bool turning = false;   // Whether or not the boid is avoiding an object

    private Vector3 random_goal;    // Random direction to move

    public List<Collider> view_set; // List of visible objects

    // Start is called before the first frame update
    void Start()
    {
        // Set a random start speed
        speed = UnityEngine.Random.Range(boidManager.minSpeed, boidManager.maxSpeed);

        // Initialize the list of visible objects
        view_set = new List<Collider>();

        // Set the random direction within the bounds of the boid manager
        var limits = boidManager.limits;
        random_goal = new Vector3(UnityEngine.Random.Range(-1 * limits.x, limits.x), UnityEngine.Random.Range(-1 * limits.y, limits.y), UnityEngine.Random.Range(-1 * limits.z, limits.z));
    }

    // Update is called once per frame
    void Update()
    {
        // Set a new random point to move towards
        if (UnityEngine.Random.Range(0, 1000) < 1)
        {
            var limits = boidManager.limits;
            random_goal = new Vector3(UnityEngine.Random.Range(-1 * limits.x, limits.x), UnityEngine.Random.Range(-1 * limits.y, limits.y), UnityEngine.Random.Range(-1 * limits.z, limits.z));
        }

        // Reset the direction change
        Vector3 direction = Vector3.zero;
        turning = false;

        // Get the other boids
        var boids = boidManager.boids;

        // Avoid objects if there are objects to avoid
        if (view_set.Count > 0)
        {
            // Add the vector away from the object for each object in view
            foreach (var view in view_set)
            {
                var surface_point = view.ClosestPoint(this.transform.position);
                var in_direction = surface_point - this.transform.position;

                direction += -in_direction.normalized * (2f - in_direction.magnitude);
            }

            turning = true;
        }

        if (turning)
        {
            // Turn away from objects
            transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), boidManager.rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Apply normal movement rules
            ApplyRules();

            // Randomize the speed
            if (UnityEngine.Random.Range(0, 100) < 10)
                speed = UnityEngine.Random.Range(boidManager.minSpeed, boidManager.maxSpeed);
        }

        // Update the position
        transform.Translate(new Vector3(0, 0, speed * Time.deltaTime));
    }

    // Add an object in view
    public void TriggerEnter(Collider other)
    {
        view_set.Add(other);
    }

    // Remove an object out of view
    public void TriggerExit(Collider other)
    {
        view_set.Remove(other);
    }

    // Modify the direction to include normal boid rules
    void ApplyRules()
    {
        // Get the boids
        var all_boids = boidManager.boids;

        // Set initial values
        Vector3 global_center = Vector3.zero;
        Vector3 vcenter = Vector3.zero;
        Vector3 vavoid = Vector3.zero;
        Vector3 valign = Vector3.zero;
        Vector3 vflatten = Vector3.zero;
        float gSpeed = 0.01f;
        float nDistance;
        var closest = new List<Tuple<Boid, float>>();

        // Randomize the number of closest boids considered
        var max_closest = UnityEngine.Random.Range(3, 20);

        // Find the closest boids
        foreach (var boid in all_boids)
        {
            global_center += boid.transform.position;

            if (boid != this)
            {
                nDistance = Vector3.Distance(boid.transform.position, this.transform.position);
                if (nDistance <= boidManager.neighbourDistance)
                {
                    // Avoid the boid if it is too close
                    var max_avoid = 1.0f;
                    if (nDistance < max_avoid)
                    {
                        var next_avoid = this.transform.position - boid.transform.position;
                        vavoid += next_avoid.normalized * (1 - (next_avoid.magnitude / max_avoid));
                    }

                    closest.Add(new Tuple<Boid, float>(boid, nDistance));
                }
            }
        }
        global_center /= all_boids.Count;
        closest.Sort(SortTupleByDistance);

        // Get a list of just the closest boids
        var boids = new List<Boid>();
        for (int i = 0; i < closest.Count && i < max_closest; ++i)
        {
            boids.Add(closest[i].Item1);
        }

        // Calculate the cohesion and alignment vectors
        // Calculate the group seed
        foreach (var boid in boids)
        {
            if (boid != this)
            {
                vcenter += boid.transform.position;
                
                valign += boid.transform.rotation * Vector3.forward;

                gSpeed += boid.speed;
            }
        }

        // Find the flatten vector
        if (boids.Count > 0)
        {
            // Initialize Matrices
            Double[,] xz_list = new Double[boids.Count, 3];
            Double[,] y_list = new Double[boids.Count, 1];
            for (int i = 0; i < boids.Count; ++i)
            {
                var boid = boids[i];

                xz_list[i, 0] = boid.transform.position.x;
                xz_list[i, 1] = boid.transform.position.z;
                xz_list[i, 2] = 1;

                y_list[i, 0] = boid.transform.position.y;
            }
            var M = Matrix<double>.Build;

            // Store Matrices
            Matrix<double> xz_mat = M.DenseOfArray(xz_list);
            Matrix<double> y_mat = M.DenseOfArray(y_list);

            // Find an estimate of the plane
            var planar_parameters = (xz_mat.Transpose() * xz_mat) * (xz_mat.Transpose() * y_mat);
            Vector3 plane_point = new Vector3(0, (float)planar_parameters[2, 0], 0);
            Vector3 plane_normal = new Vector3((float)planar_parameters[0, 0], (float)planar_parameters[2, 0], (float)planar_parameters[1, 0]);

            // Find the smallest vector from this to the plane
            vflatten = Vector3.Project(transform.position - plane_point, plane_normal);
        }

        // Update direction
        if (boids.Count > 0)
        {
            // Average out cohesion, alignment, and speed
            vcenter = (vcenter / boids.Count) - this.transform.position;
            valign = valign / boids.Count;
            speed = gSpeed / boids.Count;
            
            // Set direction
            Vector3 direction =
                  boidManager.w_alignment     * valign
                + boidManager.w_avoidance     * vavoid
                + boidManager.w_flatten       * vflatten.normalized
                + boidManager.w_center        * vcenter
                + boidManager.w_global_center * (global_center - transform.position);

            // Linearly interpolate between the ideal direction and the random direction
            direction =
                  (1 - boidManager.w_random_movement) * direction.normalized
                + (boidManager.w_random_movement)     * (random_goal - transform.position).normalized;

            // Update rotation
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), boidManager.rotationSpeed * Time.deltaTime);
            }
        } else
        {
            // Go towards the global center
            Vector3 direction = global_center - transform.position;

            // Update rotation
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), boidManager.rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    public int SortTupleByDistance(Tuple<Boid, float> a, Tuple<Boid, float> b)
    {
        return (a.Item2 - b.Item2 > 0) ? 1 : -1;
    }
}
