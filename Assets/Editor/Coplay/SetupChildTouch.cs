using UnityEngine;
using UnityEditor;

public class SetupChildTouch
{
    public static string Execute()
    {
        GameObject child = GameObject.Find("Child with text/VRChild");
        if (child == null) return "Child not found";

        // Add Script
        ChildTouchReaction reaction = child.GetComponent<ChildTouchReaction>();
        if (reaction == null) reaction = child.AddComponent<ChildTouchReaction>();

        // Add Collider (Trigger)
        CapsuleCollider collider = child.GetComponent<CapsuleCollider>();
        if (collider == null) collider = child.AddComponent<CapsuleCollider>();
        
        collider.isTrigger = true;
        // Adjust collider size/position to cover the child's body
        // Child height is ~1.5m. Center should be around 0.75m.
        collider.center = new Vector3(0, 0.75f, 0);
        collider.radius = 0.3f;
        collider.height = 1.5f;

        // Add Rigidbody to Child (Kinematic) to ensure trigger events work
        Rigidbody childRb = child.GetComponent<Rigidbody>();
        if (childRb == null) childRb = child.AddComponent<Rigidbody>();
        childRb.isKinematic = true;
        childRb.useGravity = false;

        // Ensure Main Camera has a collider for proximity detection
        if (Camera.main != null)
        {
            SphereCollider camCollider = Camera.main.GetComponent<SphereCollider>();
            if (camCollider == null) camCollider = Camera.main.gameObject.AddComponent<SphereCollider>();
            camCollider.isTrigger = true;
            camCollider.radius = 0.2f; // Head size
            
            // Ensure Rigidbody is present for trigger events (at least one object needs a Rigidbody)
            Rigidbody rb = Camera.main.GetComponent<Rigidbody>();
            if (rb == null) rb = Camera.main.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        return "Setup complete";
    }
}
