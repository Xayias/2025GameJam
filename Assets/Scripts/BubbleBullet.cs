using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleBullet : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float forwardSpeed = 5f;
    public float homingSpeed = 10f; // Speed for homing toward the enemy
    public float floatDuration = 3f; // Time before the bubble pops after capturing an enemy
    public float maxLifetime = 5f; // Maximum time the bubble will exist if it doesn't hit anything
    public float wobbleFrequency = 2f; // How fast the wobble oscillates
    public float wobbleMagnitude = 0.5f; // How much the bubble wobbles
    public Material absorbedMaterial; // Material to change to after absorbing a gas cloud
    public GameObject popEffect; // Optional: Particle effect prefab for popping
    public AudioClip popSound; // Optional: Sound effect for popping
    public float popSoundVolume = 1f;

    private Rigidbody rb;
    private bool isFloating = false; // Tracks if the bubble is in "floating" mode
    private Transform capturedEnemy = null; // Holds reference to the captured enemy
    private bool hasCapturedEnemy = false;
    private bool hasAbsorbedGas = false; // Tracks if the bubble has absorbed a gas cloud
    private GameObject targetEnemy; // The enemy to target after absorbing the gas cloud
    private Renderer bubbleRenderer; // Renderer for the bubble's material

    private float wobbleOffset = 0f; // Tracks the current horizontal wobble offset

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bubbleRenderer = GetComponent<Renderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = transform.forward * forwardSpeed;

        // Schedule the bubble to pop after maxLifetime if it doesn't hit anything
        Invoke(nameof(PopBubble), maxLifetime);
    }

    // Update is called once per frame
    void Update()
    {
        if (isFloating)
        {
            HandleFloatingMotion();
        }
        else if (hasAbsorbedGas && targetEnemy != null)
        {
            HandleHomingMotion();

            // Debug log to display the target enemy
            Debug.Log($"Bubble is homing toward: {targetEnemy.name}");
        }
        else if (hasAbsorbedGas && targetEnemy == null)
        {
            Debug.LogWarning("Bubble has absorbed gas, but targetEnemy is null!");
        }
    }

    private void HandleFloatingMotion()
    {
        // Handle upward float after collision
        float wobble = Mathf.Sin(Time.time * wobbleFrequency) * wobbleMagnitude;

        // Apply wobble to the position incrementally
        Vector3 upwardMotion = Vector3.up * floatSpeed * Time.deltaTime;
        Vector3 horizontalWobble = new Vector3(wobbleOffset, 0, 0);

        // Update the bubble's position
        transform.position += upwardMotion + horizontalWobble;

        // Keep the captured enemy attached to the bubble
        if (capturedEnemy != null)
        {
            capturedEnemy.position = transform.position; // Keep enemy inside the bubble
        }
    }

    private void HandleHomingMotion()
    {
        // Calculate the direction to the target enemy
        Vector3 direction = (targetEnemy.transform.position - transform.position).normalized;

        // Apply velocity toward the enemy
        rb.velocity = direction * homingSpeed;

        // Rotate the bubble to face the target
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prevent capturing multiple enemies
        if (hasCapturedEnemy) return;

        if (other.CompareTag("Enemy") && !hasAbsorbedGas)
        {
            hasCapturedEnemy = true;
            CaptureEnemy(other.gameObject);
        }
        else if (other.CompareTag("GermAlienEnemy") && !hasAbsorbedGas)
        {
            PopBubble();
        }
        else if (other.CompareTag("GermAlienEnemy") && hasAbsorbedGas)
        {
            if (other.gameObject == targetEnemy)
            {
                hasCapturedEnemy = true;
                CaptureEnemy(other.gameObject); // Destroy the enemy and the bubble
            }
        }
        else if (other.CompareTag("ToxicGasCloud"))
        {
            // Absorb the gas cloud
            ToxicGasCloud gasCloud = other.GetComponent<ToxicGasCloud>();
            if (gasCloud != null)
            {
                AbsorbGasCloud(gasCloud);
            }
        }
        else if (!other.CompareTag("Player") && !other.CompareTag("Enemy"))
        {
            PopBubble(); // Trigger the pop effect and destroy the bubble
        }

        if (other.CompareTag("EnemyShield"))
        {
            hasCapturedEnemy = true;
            // Capture and destroy the shield
            CaptureEnemy(other.gameObject);
        }
        else if (other.CompareTag("EnemyNeedler"))
        {
            ArmoredBossController boss = other.GetComponent<ArmoredBossController>();
            if (boss != null && boss.isVulnerable)
            {
                hasCapturedEnemy = true;
                Debug.Log("Bubble destroyed the Armored Boss!");
                Destroy(boss.gameObject); // Destroy the boss
                Destroy(gameObject); // Destroy the bubble
            }
        }
    }

    private void CaptureEnemy(GameObject enemy)
    {
        // Enter floating mode
        isFloating = true;

        // Cancel the scheduled maxLifetime pop since we�re handling a new float timer
        CancelInvoke(nameof(PopBubble));

        // Disable enemy movement and attach it to the bubble
        var enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.CaptureInBubble(transform); // Inform the enemy that it's captured
        }

        // Stop forward movement and attach the enemy
        rb.isKinematic = true; // Stop physics-based movement
        capturedEnemy = enemy.transform; // Attach the enemy to the bubble

        // Schedule the bubble to pop after a delay
        Invoke(nameof(PopBubble), floatDuration);
    }
    private void DestroyEnemy(GameObject enemy)
    {
        // Destroy the enemy and the bubble
        Destroy(enemy);
        PopBubble();
        Debug.Log("Bubble destroyed the enemy!");
    }
    private void PopBubble()
    {
        // Play pop effect if assigned
        if (popEffect != null)
        {
            Instantiate(popEffect, transform.position, Quaternion.identity);
        }

        // Play pop sound if assigned
        if (popSound != null)
        {
            AudioSource.PlayClipAtPoint(popSound, transform.position, popSoundVolume);
        }

        //transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 10f);
        // Destroy the bubble
        Destroy(gameObject);
    }

    public void AbsorbGasCloud(ToxicGasCloud gasCloud)
    {
        // Mark the bubble as having absorbed a gas cloud
        hasAbsorbedGas = true;

        // Set the target enemy as the shooter of the gas cloud
        targetEnemy = gasCloud.shooter;

        maxLifetime = 20f;

        // Change the bubble's material to indicate absorption
        if (absorbedMaterial != null && bubbleRenderer != null)
        {
            bubbleRenderer.material = absorbedMaterial;
        }

        // Destroy the gas cloud
        Destroy(gasCloud.gameObject);

        Debug.Log("Gas cloud absorbed by bubble! Targeting enemy...");
    }
}
