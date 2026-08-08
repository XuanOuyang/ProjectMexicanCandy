using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timeRemaining = 300f; // 5 minutes in seconds
    private bool gameEnded = false;

    [Header("Player Health Settings")]
    public PlayerHealth player1; // Assign Player 1 in Inspector
    public PlayerHealth player2;

    [Header("Scene Names")]
    public string winSceneName = "WinScene";
    public string loseSceneName = "LoseScene";

    void Update()
    {
        if (gameEnded) return;

        // Countdown timer
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            TriggerWin();
        }

        // Check if both players are dead
        if (player1.currentHearts <= 0 && player2.currentHearts <= 0)
        {
            TriggerLose();
        }
    }

    // Call these functions when players take damage

    void TriggerWin()
    {
        gameEnded = true;
        Debug.Log("5 minutes survived! Loading Win Scene...");
        SceneManager.LoadScene(winSceneName);
    }

    void TriggerLose()
    {
        gameEnded = true;
        Debug.Log("Both players eliminated! Loading Lose Scene...");
        SceneManager.LoadScene(loseSceneName);
    }
}