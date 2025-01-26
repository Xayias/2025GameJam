using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public string gameSceneName = "Playground"; // Name of the game scene

    public void OnStartGame(InputAction.CallbackContext context)
    {
        if (context.performed) // Only trigger when the button is pressed
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        Debug.Log("Game Started!");
        SceneManager.LoadScene(gameSceneName); // Load the main game scene
    }
}
