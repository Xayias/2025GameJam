using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

    // A static list to track all control points in the scene
    private static List<ControlPoint> allControlPoints = new List<ControlPoint>();

    private void Start()
    {
        // Clear the list if this is the first control point to initialize in the scene
        if (allControlPoints.Count == 0)
        {
            allControlPoints.Clear(); // Ensure the list is cleared on scene reload
        }

        // Initialize health
        currentHealth = maxHealth;

        // Initialize health bar
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        // Register this control point in the static list
        allControlPoints.Add(this);
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

        // Remove the destroyed control point from the list
        allControlPoints.Remove(this);

        // Check if all control points are destroyed
        CheckIfGameOver();
    }

    private static void CheckIfGameOver()
    {
        Debug.Log("Checking if all control points are destroyed...");

        // Check if all control points are destroyed
        foreach (var controlPoint in allControlPoints)
        {
            if (!controlPoint.isDestroyed)
            {
                Debug.Log($"Control point {controlPoint.name} is still active.");
                return; // At least one control point is still active
            }
        }

        Debug.Log("All control points are destroyed. Triggering game over.");
        GameManager.Instance.TriggerGameOver();
    }

    public static bool HasRemainingControlPoints()
    {
        // Check if there are any active control points
        foreach (var controlPoint in allControlPoints)
        {
            if (!controlPoint.isDestroyed)
            {
                return true;
            }
        }
        return false;
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

    public static void ClearAllControlPoints()
    {
        allControlPoints.Clear();
    }
}
