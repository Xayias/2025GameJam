using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set;}

    [Header("Game Over Settings")]
    public GameObject gameOverUI;
    public GameObject winUI;

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;

        // Show Game Over UI and restart the scene
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        Debug.Log("Game Over! Restarting...");
        Invoke(nameof(RestartScene), 3f);
    }

    public void TriggerWin()
    {
        if (isGameOver) return;

        isGameOver = true;

        // Show Win UI
        if (winUI != null)
        {
            winUI.SetActive(true);
        }

        Debug.Log("You Win!");
        // Optionally reload the scene or go to the main menu
        Invoke(nameof(RestartScene), 5f);
    }

    private void RestartScene()
    {
        // Clear the static list of control points
        ControlPoint.ClearAllControlPoints();

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
