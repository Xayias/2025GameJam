using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float speed = 3f; // Base speed for the enemy
    public float damage = 10f; // Damage dealth to control points
    public float playerDetectionRadius = 10f;
    public float stoppingDistance = 2f;

    [Header("Capture Settings")]
    public float bubbleYOffset = -0.5f; // Offset to center the enemy in the bubble

    [Header("Attack Cooldowns")]
    public float attackCooldown = 1f; // Cooldown between attacks
    public float lastAttackTime; // Time of the last attack

    [Header("References")]
    public ControlPoint targetControlPoint; // Target control point
    public Transform player; // Reference to the player's transform
    public NavMeshAgent navMeshAgent; // For navigation (optional, if using NavMesh)
    private Animator animator; // Reference to the Animator component
    private string currentAnimationState;

    public bool isCaptured = false; // Tracks if the enemy is in a bubble
    public bool isAttacking = false; // Prevents multiple attacks per second

    // Start is called before the first frame update
    void Start()
    {
        // Get the NavMeshAgent component
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent != null)
        {
            navMeshAgent.speed = speed; // Set the NavMeshAgent's speed
            navMeshAgent.stoppingDistance = stoppingDistance;
        }

        // Get the Animator component
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on this GameObject!");
        }

        // Find the player by tag
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Find the closest control point
        FindClosestControlPoint();

        // Play initial running animation
        PlayRunningAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        if (isCaptured)
        {
            PlayFloatingAnimation();
            return; // Stop all logic if captured
        }

        // If the player is within range, target them
        if (PlayerIsWithinRange())
        {
            MoveTowards(player.position);

            // Play running animation
            PlayRunningAnimation();

            // If within stopping distance of the player, attack
            if (IsWithinStoppingDistance(player.position))
            {
                AttackPlayer();
            }
        }
        // Otherwise, target the closest control point
        else if (targetControlPoint != null)
        {
            // Check if the control point is destroyed
            if (targetControlPoint.isDestroyed)
            {
                FindClosestControlPoint();
            }

            if (targetControlPoint != null && !targetControlPoint.isDestroyed)
            {
                MoveTowards(targetControlPoint.transform.position);

                // Play running animation
                PlayRunningAnimation();

                // If within stopping distance of the control point, attack
                if (IsWithinStoppingDistance(targetControlPoint.transform.position))
                {
                    AttackControlPoint();
                }
            }
            else
            {
                // No target, play idle animation
                PlayIdleAnimation();
            }
        }

        if (targetControlPoint == null)
        {
            Debug.LogWarning("EnemyController: No target control point found.");
        }
        else if (targetControlPoint.isDestroyed)
        {
            Debug.LogWarning($"EnemyController: Target control point {targetControlPoint.name} is destroyed.");
        }
    }

    private bool PlayerIsWithinRange()
    {
        // Check if the player is within the detection radius
        if (player == null) return false; // Ensure player exists
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= playerDetectionRadius;
    }

    private bool IsWithinStoppingDistance(Vector3 targetPosition)
    {
        // Check if the enemy is within stopping distance of the target
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        return distanceToTarget <= stoppingDistance;
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

        transform.SetParent(bubbleTransform); // Attach to bubble

        // Adjust the local position of the main enemy object (default offset)
        float defaultYOffset = -0.5f;
        transform.localPosition = new Vector3(0, defaultYOffset, 0);

        // Find the child object with the specific tag (e.g., "EnemyModel")
        Transform childWithTag = GetChildWithTag("yOffset"); // Change "EnemyModel" to your tag
        if (childWithTag != null)
        {
            float childYOffset = -1f; // Adjust this offset for the child object
            childWithTag.localPosition = new Vector3(
                childWithTag.localPosition.x, 
                childYOffset, 
                childWithTag.localPosition.z
            );

            Debug.Log($"Adjusted child '{childWithTag.name}' Y Offset: {childYOffset}");
        }
        else
        {
            Debug.LogWarning("Child with tag 'EnemyModel' not found!");
        }

        // Debug log to confirm the position
        Debug.Log($"Enemy transform info:\n" +
                  $"- World Position: {transform.position}\n" +
                  $"- Local Position: {transform.localPosition}\n" +
                  $"- Parent: {transform.parent.name}");
    }

    // Utility function to find a child with a specific tag
    private Transform GetChildWithTag(string tag)
    {
        foreach (Transform child in transform) // Iterate through children of this object
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
        }
        return null; // Return null if no child with the tag is found
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        if (navMeshAgent != null)
        {
            // Use NavMeshAgent for movement
            navMeshAgent.SetDestination(targetPosition);
        }
        else
        {
            // Fallback: Use basic movement if NavMeshAgent is not available
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    private void FindClosestControlPoint()
    {
        // If no control points remain, stop all behavior
        if (!ControlPoint.HasRemainingControlPoints())
        {
            StopAllBehavior();
            return;
        }

        // Find all control points in the scene
        ControlPoint[] controlPoints = FindObjectsOfType<ControlPoint>();
        float shortestDistance = Mathf.Infinity;

        foreach (ControlPoint controlPoint in controlPoints)
        {
            if (controlPoint.isDestroyed) continue; // Skip destroyed control points

            float distance = Vector3.Distance(transform.position, controlPoint.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                targetControlPoint = controlPoint;
            }
        }

        if (targetControlPoint == null)
        {
            // No valid control points remain
            StopAllBehavior();
        }
    }

    private void AttackControlPoint()
    {
        if (isAttacking || targetControlPoint == null || targetControlPoint.isDestroyed)
        {
            return; // Prevent attacking destroyed or null control points
        }

        isAttacking = true;

        // Damage the control point
        targetControlPoint.TakeDamage(damage);
        Debug.Log($"Attacking Control Point: {targetControlPoint.name}");

        // Allow another attack after a short delay (e.g., 1 second)
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void ResetAttack()
    {
        isAttacking = false;
    }

    private void StopAllBehavior()
    {
        Debug.Log("All control points are destroyed. Enemy is idle.");
        // Optional: Add idle animation or logic here
        enabled = false; // Disable this script to stop enemy behavior
    }

    private void AttackPlayer()
    {
        if (Time.time < lastAttackTime + attackCooldown) return; // Prevent multiple attacks

        // Play attack animation
        PlayAttackAnimation();

        // Get the PlayerHealth component from the player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage); // Deal damage to the player
            Debug.Log("Attacking Player!");
        }

        lastAttackTime = Time.time; // Update last attack time
    }

    private void ChangeAnimationState(string newState)
    {
        // Prevent interrupting the same animation
        if (currentAnimationState == newState) return;
    
        // Update the animator with the new state
        animator.Play(newState);
    
        // Update the current animation state
        currentAnimationState = newState;
    }

    // Animation Methods
    private void PlayRunningAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", true);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isFloating", false);
        }
    }

    private void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", true);
            animator.SetBool("isFloating", false);
        }
    }

    private void PlayIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isFloating", false);
        }
    }

    private void PlayFloatingAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isFloating", true);
        }
    }
}
