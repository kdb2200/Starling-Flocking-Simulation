using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public BoidManager boidManager;     // Boid Manager for average center
    public float cameraSpeed = 1.0f;    // Relative speed of the camera

    // Start is called before the first frame update
    void Start()
    {
        // Look at the center of the boids
        transform.rotation = Quaternion.LookRotation(boidManager.center - transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        // Get the distance away from the center for relative movements
        var delta_magnitude = (boidManager.center - transform.position).magnitude;

        if (Input.GetKey("a") && Input.GetKey("d")) { }
        else if (Input.GetKey("a"))
        {
            // Move the camera relatively left
            this.transform.position += transform.rotation * Vector3.left * Time.deltaTime * delta_magnitude;
        }
        else if (Input.GetKey("d"))
        {
            // Move the camera relatively right
            this.transform.position += transform.rotation * Vector3.right * Time.deltaTime * delta_magnitude;
        }

        if (Input.GetKey("w") && Input.GetKey("s")) { }
        else if (Input.GetKey("w"))
        {
            // Move the camera relatively up
            this.transform.position += transform.rotation * Vector3.up * Time.deltaTime * delta_magnitude;
        }
        else if (Input.GetKey("s"))
        {
            // Move the camera relatively down
            this.transform.position += transform.rotation * Vector3.down * Time.deltaTime * delta_magnitude;
        }

        if (Input.mouseScrollDelta.y != 0f)
        {
            // Move the camera relatively forward and backward
            this.transform.position += transform.rotation * Vector3.forward * Input.mouseScrollDelta.y;
        }

        // Update the rotation to point at the boid center for the new position
        transform.rotation = Quaternion.LookRotation(boidManager.center - transform.position);
    }
}
