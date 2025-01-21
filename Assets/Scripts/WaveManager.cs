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

        // Display the "Wave Starting" text on the UI
        currentWave++;
        waveUIText.text = $"Wave {currentWave} Starting!";
        waveUIText.gameObject.SetActive(true);

        // Wait 2 seconds to display the wave starting message
        yield return new WaitForSeconds(2f);

        waveUIText.gameObject.SetActive(false);

        // Spawn the enemies for this wave
        StartCoroutine(SpawnEnemiesForWave(currentWave));

        // Wait until all enemies are cleared
        while (AreEnemiesRemaining())
        {
            yield return null; // Wait until all enemies are gone
        }

        // Check if it's the last wave
        if (currentWave >= totalWaves)
        {
            EndGame(true); // Game won after the 10th wave
        }
        else
        {
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

    private IEnumerator SpawnEnemiesForWave(int waveNumber)
    {
        for (int i = 0; i < enemyPrefabs.Length; i++) // Iterate through enemy types
        {
            int enemiesToSpawn = enemiesPerWave[i]; // Get the number of this enemy type to spawn
            for (int j = 0; j < enemiesToSpawn; j++)
            {
                // Spawn at a random spawn point
                SpawnPoint spawnPoint = GetRandomSpawnPoint();
                spawnPoint.SpawnEnemy(enemyPrefabs[i]);

                // Optional delay between enemy spawns
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private SpawnPoint GetRandomSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
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
