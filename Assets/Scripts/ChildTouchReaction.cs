using UnityEngine;

public class ChildTouchReaction : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator not found on ChildTouchReaction object.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (animator == null) return;

        // Check if the object colliding is the player (Camera or Hands)
        // You might need to adjust these checks based on your project's tags/layers
        if (other.CompareTag("MainCamera") || 
            other.CompareTag("Player") || 
            other.name.Contains("Hand") || 
            other.name.Contains("Controller") ||
            other.name.Contains("Interactor"))
        {
            Debug.Log("Child touched by: " + other.name);
            animator.SetTrigger("IsHappy");
        }
    }
}
