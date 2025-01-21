using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public enum SpawnFrequency { Slow, Normal, Fast } // Toggle options
    public SpawnFrequency spawnFrequency; // Frequency setting

    public void SpawnEnemy(GameObject enemyPrefab)
    {
        Instantiate(enemyPrefab, transform.position, Quaternion.identity);
    }

    public float GetSpawnRate()
    {
        // Set spawn rates based on frequency
        switch (spawnFrequency)
        {
            case SpawnFrequency.Slow:
                return 1f; // Slow rate (e.g., spawn every 1 second)
            case SpawnFrequency.Normal:
                return 0.5f; // Normal rate (e.g., spawn every 0.5 seconds)
            case SpawnFrequency.Fast:
                return 0.2f; // Fast rate (e.g., spawn every 0.2 seconds)
            default:
                return 1f; // Default to slow if unset
        }
    }
}
