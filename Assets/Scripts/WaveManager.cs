using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public int totalWaves = 10; // Total number of waves
    public float timeBetweenWaves = 15f; // Time between waves
    public Text waveUIText; // UI Text for displaying wave info
    public Text countdownUIText; // UI Text for the countdown timer

    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs; // Array of enemy types
    public int[] enemiesPerWave; // Number of enemies to spawn per type for each wave (Set this in the inspector)

    [Header("Spawn Point Settings")]
    public SpawnPoint[] spawnPoints; // Array of spawn points

    [Header("Wave Configurations")]
    public WaveConfiguration[] waves; // Array of wave configurations

    [Header("Health Pickup Settings")]
    public GameObject healthPickupPrefab; // Reference to the health pickup prefab
    public Transform[] healthPickupSpawnPoints; // Array of spawn points for health pickups

    private int currentWave = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartNextWave());
    }

    private void SpawnHealthPickup()
    {
        if (healthPickupPrefab == null || healthPickupSpawnPoints.Length == 0) return;
    
        // Get a random spawn point
        Transform randomSpawnPoint = healthPickupSpawnPoints[Random.Range(0, healthPickupSpawnPoints.Length)];
    
        // Apply an offset to lift the pickup above the ground
        Vector3 spawnPosition = randomSpawnPoint.position + new Vector3(0, 0.5f, 0); // Adjust Y offset (e.g., 0.5f)

        // Check if the player's health is not full before spawning
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null && playerHealth.currentHealth < playerHealth.maxHealth)
        {
            // Spawn the health pickup at the random spawn point
            Instantiate(healthPickupPrefab, randomSpawnPoint.position, Quaternion.identity);
            Debug.Log("Health pickup spawned!");
        }
    }

    private IEnumerator StartNextWave()
    {
        // Display the "Wave Starting" text on the UI
        waveUIText.text = $"Wave {currentWave + 1} Starting!";
        waveUIText.gameObject.SetActive(true);

        // Wait 2 seconds to display the wave starting message
        yield return new WaitForSeconds(2f);

        waveUIText.gameObject.SetActive(false);

        // Spawn health pickup every 3 waves
        if ((currentWave + 1) % 3 == 0)
        {
            SpawnHealthPickup();
        }

        // Spawn the enemies for this wave
        yield return StartCoroutine(SpawnEnemiesForWave(currentWave));

        // Wait until all enemies are cleared
        while (AreEnemiesRemaining())
        {
            yield return null; // Wait until all enemies are gone
        }

        // Check if it's the last wave
        if (currentWave >= totalWaves - 1)
        {
            Debug.Log("WaveManager Script: Win Game");
            GameManager.Instance.TriggerWin();
        }
        else
        {
            // increment wave counter
            currentWave++; // Only increment AFTER the current wave is fully completed

            // Start the countdown before the next wave
            StartCoroutine(CountdownBeforeNextWave());
        }
    }

    private IEnumerator CountdownBeforeNextWave()
    {
        float countdown = timeBetweenWaves;
        countdownUIText.gameObject.SetActive(true);

        while (countdown > 0)
        {
            countdownUIText.text = $"Next Wave in {Mathf.Ceil(countdown)} seconds...";
            countdown -= Time.deltaTime;
            yield return null;
        }

        countdownUIText.gameObject.SetActive(false);

        // Start the next wave
        StartCoroutine(StartNextWave());
    }

    private SpawnPoint GetAvailableSpawnPoint()
    {
        // Get all spawn points that are available (CanSpawn() == true)
        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.CanSpawn())
            {
                availableSpawnPoints.Add(spawnPoint);
            }
        }

        // If no spawn points are available, return null (WaveManager will wait)
        if (availableSpawnPoints.Count == 0) return null;

        // Return a random available spawn point
        return availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
    }

    private IEnumerator SpawnEnemiesForWave(int waveIndex)
    {

        // Get the current wave configuration
        if (waveIndex < 0 || waveIndex >= waves.Length) yield break;
        WaveConfiguration currentWave = waves[waveIndex];

        // Loop through each enemy type in the wave
        for (int i = 0; i < currentWave.enemyCounts.Length; i++)
        {
            int enemiesToSpawn = currentWave.enemyCounts[i]; // Number of enemies for this type

            for (int j = 0; j < enemiesToSpawn; j++)
            {
                SpawnPoint spawnPoint = null;

                // Wait until a spawn point is available
                while (spawnPoint == null)
                {
                    spawnPoint = GetAvailableSpawnPoint();
                    yield return null; // Wait a frame and check again
                }

                // Spawn the enemy at the available spawn point
                spawnPoint.SpawnEnemy(enemyPrefabs[i]);

                // Optional: Add a delay between spawns for smoother pacing
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private bool AreEnemiesRemaining()
    {
        // Find all enemies with the "Enemy" tag
        int enemiesWithTagEnemy = GameObject.FindGameObjectsWithTag("Enemy").Length;

        // Find all enemies with the "GermAlienEnemy" tag
        int enemiesWithTagGermAlien = GameObject.FindGameObjectsWithTag("GermAlienEnemy").Length;

        // Find all enemies with the "Enemy" tag
        int enemiesWithTagEnemyNeedler = GameObject.FindGameObjectsWithTag("EnemyNeedler").Length;

        // Return true if there are any enemies with either tag
        return (enemiesWithTagEnemy > 0 || enemiesWithTagGermAlien > 0 || enemiesWithTagEnemyNeedler > 0);
    }
}
