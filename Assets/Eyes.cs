using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eyes : MonoBehaviour
{
    private Boid parent;

    // Start is called before the first frame update
    void Start()
    {
        // Store the parent boid
        parent = transform.parent.GetComponent<Boid>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Add a collider to the known objects
    void OnTriggerEnter(Collider other)
    {
        if (parent == null) return;

        parent.TriggerEnter(other);
    }

    // Remove a collider from the known objects
    void OnTriggerExit(Collider other)
    {
        if (parent == null) return;

        parent.TriggerExit(other);
    }
}
