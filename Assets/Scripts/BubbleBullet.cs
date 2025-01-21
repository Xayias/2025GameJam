using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleBullet : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float forwardSpeed = 5f;
    public float floatDuration = 3f; // Time before the bubble pops after capturing an enemy
    public float maxLifetime = 5f;   // Maximum time the bubble will exist if it doesn't hit anything
    public float wobbleFrequency = 2f; // How fast the wobble oscillates
    public float wobbleMagnitude = 0.5f; // How much the bubble wobbles

    public GameObject popEffect; // Optional: Particle effect prefab for popping
    public AudioClip popSound;   // Optional: Sound effect for popping
    public float popSoundVolume = 1f;

    private Rigidbody rb;
    private bool isFloating = false; // Tracks if the bubble is in "floating" mode
    private Transform capturedEnemy = null; // Holds reference to the captured enemy

    private float wobbleOffset = 0f; // Tracks the current horizontal wobble offset

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // Enable physics
        rb.velocity = transform.forward * forwardSpeed; // Initial forward movement

        // Schedule the bubble to pop after maxLifetime if it doesn't hit anything
        Invoke(nameof(PopBubble), maxLifetime);
    }

    // Update is called once per frame
    void Update()
    {
        if (isFloating)
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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && !isFloating)
        {
            CaptureEnemy(other.gameObject);
        }
        // If the bubble hits anything else (not Player or Enemy), pop it
        else if (!other.CompareTag("Player") && !other.CompareTag("Enemy"))
        {
            PopBubble(); // Trigger the pop effect and destroy the bubble
        }
    }

    private void CaptureEnemy(GameObject enemy)
    {
        // Enter floating mode
        isFloating = true;

        // Cancel the scheduled maxLifetime pop since we’re handling a new float timer
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

        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 10f);
        // Destroy the bubble
        Destroy(gameObject);
    }
}
