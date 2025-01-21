using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private bool isCaptured = false; // Tracks if the enemy is in a bubble
    private Rigidbody rb;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    public void CaptureInBubble(Transform bubbleTransform)
    {
        if (isCaptured) return; // Prevent double capturing

        isCaptured = true;
        rb.isKinematic = true; // Stop physics-based movement
        rb.velocity = Vector3.zero; // Stop any momentum
        transform.SetParent(bubbleTransform); // Attach to bubble

        // Optional: Trigger animations or effects
        if (animator != null)
        {
            animator.Play("Captured");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCaptured)
        {
            // Basic movement toward the player
            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            Vector3 direction = (player.position - transform.position).normalized;
            rb.MovePosition(transform.position + direction * Time.deltaTime * 2f);
        }
    }
}
