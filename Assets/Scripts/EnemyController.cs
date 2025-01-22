using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public float speed = 3f; // Base speed for the enemy
    private Transform player; // Reference to the player's transform
    private NavMeshAgent navMeshAgent; // For navigation (optional, if using NavMesh)

    private bool isCaptured = false; // Tracks if the enemy is in a bubble

    //private Rigidbody rb;
    //private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody>();
        //animator = GetComponent<Animator>();

        player = GameObject.FindGameObjectWithTag("Player").transform; // Find the player by tag
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent != null)
        {
            navMeshAgent.speed = speed; // Set the NavMeshAgent's speed
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isCaptured) return; // Do nothing if captured in a bubble

        if (player != null)
        {
            if (navMeshAgent != null)
            {
                // Use NavMeshAgent for navigation
                navMeshAgent.SetDestination(player.position);
            }
            else
            {
                // Use basic movement logic if NavMesh is not used
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
            }
        }
        //if (!isCaptured)
        //{
            // Basic movement toward the player
            //Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            //Vector3 direction = (player.position - transform.position).normalized;
            //rb.MovePosition(transform.position + direction * Time.deltaTime * 2f);
        //}
    }

    public void CaptureInBubble(Transform bubbleTransform)
    {
        if (isCaptured) return; // Prevent double capturing

        isCaptured = true;

        // Stop the enemy's movement
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
        }
        else
        {
            speed = 0; // Stop basic movement
        }

        //rb.isKinematic = true; // Stop physics-based movement
        //rb.velocity = Vector3.zero; // Stop any momentum
        transform.SetParent(bubbleTransform); // Attach to bubble
        transform.localPosition = Vector3.zero;

        // Optional: Trigger animations or effects
        //if (animator != null)
        //{
        //    animator.Play("Captured");
        //}
    }

    // Optional: Call this to set the enemy's speed dynamically
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;

        if (navMeshAgent != null)
        {
            navMeshAgent.speed = newSpeed;
        }
    }
}
