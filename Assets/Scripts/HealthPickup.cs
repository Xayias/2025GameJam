using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public float healAmount = 50f;
    public float spinSpeed = 100f;
    public float floatAmplitude = 0.2f; // Height of the floating effect
    public float floatFrequency = 1f; // Speed of the floating effect
    public float tiltAmplitude = 15f; // Maximum tilt angle
    public float tiltFrequency = 1f; // Speed of the tilting effect

    private Vector3 startPosition;

    private void Update()
    {
        // Make the green block spin
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);

        // Apply the floating effect
        float floatOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = startPosition + new Vector3(0, floatOffset, 0);

        // Apply the tilting effect (rotating side to side)
        float tiltAngle = Mathf.Sin(Time.time * tiltFrequency) * tiltAmplitude;
        transform.localRotation = Quaternion.Euler(tiltAngle, transform.localRotation.eulerAngles.y, tiltAngle);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Heal the player
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (playerHealth.currentHealth < playerHealth.maxHealth)
                {
                    playerHealth.Heal(healAmount);
                    Debug.Log("Player healed by health pickup");

                    // Destroy the health pickup after being healed
                    Destroy(gameObject);
                }
                else {
                    Debug.Log("Player is already at full health");
                }
            }
        }
    }
}
