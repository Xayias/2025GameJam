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
    public GameObject gameOverUI; // UI to show when the game ends

    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs; // Array of enemy types
    public int[] enemiesPerWave; // Number of enemies to spawn per type for each wave (Set this in the inspector)

    [Header("Spawn Point Settings")]
    public SpawnPoint[] spawnPoints; // Array of spawn points

    [Header("Wave Configurations")]
    public WaveConfiguration[] waves; // Array of wave configurations

    private int currentWave = 0;
    private bool isGameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartNextWave());
    }

    private IEnumerator StartNextWave()
    {
        // If the game is over, exit
        if (isGameOver)
            yield break;

        Debug.Log($"Starting Wave {currentWave + 1}");

        // Display the "Wave Starting" text on the UI
        waveUIText.text = $"Wave {currentWave + 1} Starting!";
        waveUIText.gameObject.SetActive(true);

        // Wait 2 seconds to display the wave starting message
        yield return new WaitForSeconds(2f);

        waveUIText.gameObject.SetActive(false);

        // Spawn the enemies for this wave
        yield return StartCoroutine(SpawnEnemiesForWave(currentWave));
        Debug.Log($"All enemies for Wave {currentWave} spawned.");

        // Wait until all enemies are cleared
        while (AreEnemiesRemaining())
        {
            yield return null; // Wait until all enemies are gone
        }
        Debug.Log($"Wave {currentWave} complete.");

        // Check if it's the last wave
        if (currentWave >= totalWaves - 1)
        {
            EndGame(true); // Game won after the 10th wave
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
        Debug.Log($"Spawning enemies for Wave {waveIndex}");

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

                Debug.Log($"Spawned enemy {enemyPrefabs[i].name} at {spawnPoint.name}");

                // Optional: Add a delay between spawns for smoother pacing
                yield return new WaitForSeconds(0.5f);
            }
        }
        Debug.Log($"Finished spawning enemies for Wave {waveIndex}");
    }

    private bool AreEnemiesRemaining()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length > 0;
    }

    private void EndGame(bool hasWon)
    {
        isGameOver = true;

        if (hasWon)
        {
            waveUIText.text = "You Win!";
        }
        else
        {
            waveUIText.text = "Game Over!";
        }

        waveUIText.gameObject.SetActive(true);
        gameOverUI.SetActive(true);
    }

    // Call this when the player dies
    private void PlayerDied()
    {
        // Uncomment this when implementing player health
        // EndGame(false);
    }

    // Call this when all control points are destroyed
    private void ControlPointsDestroyed()
    {
        // Uncomment this when implementing control point logic
        // EndGame(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
