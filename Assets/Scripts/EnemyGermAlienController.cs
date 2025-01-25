using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyGermAlienController : EnemyController
{
    [Header("Toxic Gas Settings")]
    public GameObject toxicGasPrefab; // Prefab for the toxic gas cloud
    public Transform attackPoint; // Point where the gas cloud spawns
    public float attackRange = 10f; // Distance from target to attack
    public float toxicGasSpeed = 5f; // Speed of the gas cloud

    private void Start()
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

    void Update()
    {
        if (isCaptured) return; // Stop all logic if captured

        // Check for player or control point targeting
        if (PlayerIsWithinRange())
        {
            MoveTowards(player.position, stopBeforeTarget: true);

            // Attack the player if within range
            if (IsWithinAttackRange(player.position))
            {
                AttackWithToxicGas(player.position);
            }
        }
        else if (targetControlPoint != null)
        {
            // Check if the control point is destroyed
            if (targetControlPoint.isDestroyed)
            {
                FindClosestControlPoint();
            }

            if (targetControlPoint != null && !targetControlPoint.isDestroyed)
            {
                MoveTowards(targetControlPoint.transform.position, stopBeforeTarget: true);

                // Attack the control point if within range
                if (IsWithinAttackRange(targetControlPoint.transform.position))
                {
                    AttackWithToxicGas(targetControlPoint.transform.position);
                }
            }
        }
    }

    private bool PlayerIsWithinRange()
    {
        // Check if the player is within the detection radius
        if (player == null) return false; // Ensure player exists
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= playerDetectionRadius;
    }

    private void MoveTowards(Vector3 targetPosition, bool stopBeforeTarget)
    {
        if (navMeshAgent != null)
        {
            // Set the NavMesh destination
            navMeshAgent.SetDestination(targetPosition);

            // Stop the enemy before getting too close
            if (stopBeforeTarget && IsWithinStoppingDistance(targetPosition))
            {
                navMeshAgent.isStopped = true;
            }
            else
            {
                navMeshAgent.isStopped = false;
            }
        }
    }

    private bool IsWithinAttackRange(Vector3 targetPosition)
    {
        // Check if the target is within attack range
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        return distanceToTarget <= attackRange;
    }

    private bool IsWithinStoppingDistance(Vector3 targetPosition)
    {
        // Check if the enemy is within stopping distance of the target
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        return distanceToTarget <= stoppingDistance;
    }

    private void AttackWithToxicGas(Vector3 targetPosition)
    {
        if (Time.time < lastAttackTime + attackCooldown) return; // Prevent spamming attacks

        if (toxicGasPrefab != null && attackPoint != null)
        {
            // Spawn the toxic gas cloud
            GameObject gasCloud = Instantiate(toxicGasPrefab, attackPoint.position, Quaternion.identity);

            Debug.Log($"Gas cloud instantiated: {gasCloud.name}");

            // Calculate the direction to the target
            Vector3 direction = (targetPosition - attackPoint.position).normalized;
            gasCloud.transform.forward = direction;

            // Assign the shooter to the gas cloud
            ToxicGasCloud gasCloudScript = gasCloud.GetComponent<ToxicGasCloud>();
            if (gasCloudScript != null)
            {
                gasCloudScript.shooter = gameObject; // Assign this enemy as the shooter
                Debug.Log($"Assigned shooter: {gameObject.name} to ToxicGasCloud");
            }
            else
            {
                Debug.LogError("ToxicGasCloud script not found on the instantiated gas cloud!");
            }
        }
        else
        {
            Debug.LogError("ToxicGasPrefab or AttackPoint is not assigned!");
        }

        lastAttackTime = Time.time; // Update the last attack time
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

    private void StopAllBehavior()
    {
        Debug.Log("All control points are destroyed. Enemy is idle.");
        // Optional: Add idle animation or logic here
        enabled = false; // Disable this script to stop enemy behavior
    }
}
