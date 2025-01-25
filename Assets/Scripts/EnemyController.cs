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

    [Header("Attack Cooldowns")]
    public float attackCooldown = 1f; // Cooldown between attacks
    public float lastAttackTime; // Time of the last attack

    [Header("References")]
    public ControlPoint targetControlPoint; // Target control point
    public Transform player; // Reference to the player's transform
    public NavMeshAgent navMeshAgent; // For navigation (optional, if using NavMesh)

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

        // Find the player by tag
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Find the closest control point
        FindClosestControlPoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (isCaptured) return; // Stop all logic if captured

        // If the player is within range, target them
        if (PlayerIsWithinRange())
        {
            MoveTowards(player.position);

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

                // If within stopping distance of the control point, attack
                if (IsWithinStoppingDistance(targetControlPoint.transform.position))
                {
                    AttackControlPoint();
                }
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
        transform.localPosition = Vector3.zero;

        // Optional: Trigger animations or effects
        //if (animator != null)
        //{
        //    animator.Play("Captured");
        //}
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

        // Get the PlayerHealth component from the player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage); // Deal damage to the player
            Debug.Log("Attacking Player!");
        }

        lastAttackTime = Time.time; // Update last attack time
    }
}
