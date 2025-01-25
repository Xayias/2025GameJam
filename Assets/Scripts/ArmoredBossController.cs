using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ArmoredBossController : MonoBehaviour
{
    [Header("Boss Settings")]
    public float attackCooldown = 3f; // Cooldown for throwing projectiles
    public GameObject spearPrefab; // Projectile prefab (e.g., spear)
    public Transform spearSpawnPoint; // Point where projectiles are spawned
    public float spearSpeed = 8f; // Speed of the projectile
    public bool isVulnerable = false; // Tracks if the boss is vulnerable
    public float rangedAttackDistance = 10f; // Distance from the player to stop moving
    public float circleRadius = 5f; // Distance to maintain around the player for spreading
    public float circleRotationSpeed = 30f; // Speed of circling around the player

    [Header("Shield Settings")]
    public List<GameObject> shields; // List of shield GameObjects
    public Material vulnerableMaterial; // Material to show the boss is vulnerable
    private Renderer bossRenderer;

    public float moveSpeed = 3f;
    private Transform player; // Reference to the player's transform
    private NavMeshAgent navMeshAgent; // NavMeshAgent for movement
    private float lastAttackTime; // Tracks time of the last attack
    private float assignedAngle = 0f; // Assigned angle for this boss around the player

    // Start is called before the first frame update
    void Start()
    {
        // Get references
        player = GameObject.FindGameObjectWithTag("Player").transform;
        navMeshAgent = GetComponent<NavMeshAgent>();
        bossRenderer = GetComponent<Renderer>();

        // Assign a random angle for this boss around the player
        assignedAngle = Random.Range(0f, 360f);

        // Configure NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.stoppingDistance = rangedAttackDistance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        // Move toward the player and circle around them
        MoveAndCirclePlayer();

        // Check if all shields are destroyed
        CheckShields();

        // Throw spears at bubbles
        HandleSpearAttack();
    }

    private void MoveAndCirclePlayer()
    {
        if (isVulnerable) return; // Don't move or circle if the boss is vulnerable

        // Calculate the position this boss should move to (circle around the player)
        Vector3 targetPosition = GetCirclePositionAroundPlayer();

        // Move to the target position
        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(targetPosition);
        }
    }

    private Vector3 GetCirclePositionAroundPlayer()
    {
        // Gradually adjust the assigned angle to make the boss circle the player
        assignedAngle += circleRotationSpeed * Time.deltaTime;

        // Convert the angle to radians
        float angleInRadians = assignedAngle * Mathf.Deg2Rad;

        // Calculate the position around the player in a circle
        Vector3 offset = new Vector3(
            Mathf.Cos(angleInRadians) * circleRadius,
            0f,
            Mathf.Sin(angleInRadians) * circleRadius
        );

        return player.position + offset; // Target position is offset from the player's position
    }

    private void CheckShields()
    {
        // If already vulnerable, no need to check
        if (isVulnerable) return;

        // Check if all shields are destroyed
        for (int i = shields.Count - 1; i >= 0; i--)
        {
            if (shields[i] == null)
            {
                shields.RemoveAt(i);
            }
        }

        if (shields.Count == 0)
        {
            BecomeVulnerable();
        }
    }

    private void BecomeVulnerable()
    {
        isVulnerable = true;

        // Change the boss's materal to show vulnerability
        if (vulnerableMaterial != null && bossRenderer != null)
        {
            bossRenderer.material = vulnerableMaterial;
        }

        Debug.Log("Armored Boss is now vulnerable");
    }

    private void HandleSpearAttack()
    {
        // Only attack if enough time has passed since the last attack
        if (Time.time < lastAttackTime + attackCooldown) return;

        // Find the nearest bubble or player
        GameObject target = FindNearestBubble();
        if (target != null && spearPrefab != null)
        {
            // Spawn the projectile and aim it at the target
            GameObject projectile = Instantiate(spearPrefab, spearSpawnPoint.position, Quaternion.identity);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 direction = (target.transform.position - spearSpawnPoint.position).normalized;
                rb.velocity = direction * spearSpeed;
            }

            Debug.Log($"Boss attacks {target.name}!");
        }

        lastAttackTime = Time.time;
    }

    private GameObject FindNearestBubble() 
    {
        // Find all bubble in the scene
        GameObject[] bubbles = GameObject.FindGameObjectsWithTag("Bubble");

        GameObject nearestBubble = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject bubble in bubbles)
        {
            float distance = Vector3.Distance(transform.position, bubble.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestBubble = bubble;
            }
        }

        return nearestBubble;
    }

    private void ThrowSpear(Vector3 targetPosition)
    {
        if (spearPrefab == null || spearSpawnPoint == null) return;

        // Spawn the spear
        GameObject spear = Instantiate(spearPrefab, spearSpawnPoint.position, Quaternion.identity);

        // Set the spear's velocity toward the target
        Rigidbody rb = spear.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (targetPosition - spearSpawnPoint.position).normalized;
            rb.velocity = direction * spearSpeed;
        }

        Debug.Log("Spear thrown at bubble!");
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the boss is vulnetable, allow destruction by a bubble
        if (isVulnerable && other.CompareTag("Bubble"))
        {
            Debug.Log("Boss hit by bubble! Destroying boss...");
            Destroy(gameObject); // Destroy the boss
        }
    }
}
