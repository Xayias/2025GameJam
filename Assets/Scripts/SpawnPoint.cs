using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public enum SpawnFrequency { Slow, Normal, Fast } // Toggle options
    public SpawnFrequency spawnFrequency; // Frequency setting

    [Header("Spawn Settings")]
    public float spawnRadius = 5f; // Radius to check for existing enemies
    private float cooldownTime = 0f; // Time before this spawn point can spawn again
    private float nextSpawnTime = 0f; // Tracks when this spawn point is available

    private void Start()
    {
        // Set cooldown times based on frequency
        switch (spawnFrequency)
        {
            case SpawnFrequency.Slow:
                cooldownTime = 30f;
                break;
            case SpawnFrequency.Normal:
                cooldownTime = 15f;
                break;
            case SpawnFrequency.Fast:
                cooldownTime = 5f;
                break;
        }
    }

    public bool CanSpawn()
    {
        // Check if this spawn point is off cooldown
        if (Time.time < nextSpawnTime) return false;

        // Check if an enemy is within the spawn radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, spawnRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                return false; // Enemy is within the spawn radius
            }
        }

        return true; // Spawn point is available
    }

    public void SpawnEnemy(GameObject enemyPrefab)
    {
        // Spawn the enemy
        Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        // Reset the cooldown
        nextSpawnTime = Time.time + cooldownTime;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the spawn radius in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
