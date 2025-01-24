using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControlPoint : MonoBehaviour
{
    [Header("Control Point Settings")]
    public float maxHealth = 200f; // Max health for the control point
    private float currentHealth; // Current health

    [Header("UI Settings")]
    public Slider healthBar; // UI health bar

    [Header("Attack Settings")]
    public float attackCooldown = 1f; // Cooldown between enemy attacks
    private float lastAttackTime;

    [Header("Destroyed Settings")]
    public GameObject destroyedMesh; // Mesh to display when destroyed
    public GameObject activeMesh; // The normal active mesh
    public bool isDestroyed = false; // Tracks if this control point is destroyed

    private static int activeControlPoints = 0; // Tracks how many control points remain

    private void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Initialize health bar
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        // Increment the count of active control points
        activeControlPoints++;
    }

    private void Update()
    {
        // Update the health bar UI
        if (healthBar != null && !isDestroyed)
        {
            healthBar.value = currentHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDestroyed) return;

        // Reduce health
        currentHealth -= damage;

        // Clamp health to avoid negative values
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Check if control point is destroyed
        if (currentHealth <= 0 && !isDestroyed)
        {
            OnDestroyed();
        }
    }

    private void OnDestroyed()
    {
        isDestroyed = true;

        // Disable the health bar
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }

        // Switch to the destroyed mesh
        if (activeMesh != null) activeMesh.SetActive(false);
        if (destroyedMesh != null) destroyedMesh.SetActive(true);

        // Decrement the number of active control points
        activeControlPoints--;

        // Check if all control points are destroyed
        if (activeControlPoints <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        Debug.Log("Game Over! All control points have been destroyed.");
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
        // Add your game over logic here (e.g., show Game Over UI, stop the game, etc.)
        // Example: GameManager.Instance.GameOver();
    }

    public static bool HasRemainingControlPoints()
    {
        return activeControlPoints > 0;
    }

    public bool CanBeAttacked()
    {
        // Check if the control point can be attacked based on cooldown
        return Time.time >= lastAttackTime + attackCooldown;
    }

    public void OnAttacked(float damage)
    {
        if (CanBeAttacked())
        {
            TakeDamage(damage);
            lastAttackTime = Time.time;
        }
    }
}
