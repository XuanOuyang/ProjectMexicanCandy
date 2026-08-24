using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Needed for TextMeshPro

public class GameManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timeRemaining = 300f; // 5 minutes in seconds
    public TextMeshProUGUI timerText; // Drag your UI Text component here
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
            UpdateTimerDisplay(timeRemaining);
        }
        else
        {
            timeRemaining = 0;
            UpdateTimerDisplay(timeRemaining);
            TriggerWin();
        }

        // Check if both players are dead (with null check for safety)
        if (player1 != null && player2 != null)
        {
            if (player1.currentHearts <= 0 && player2.currentHearts <= 0)
            {
                TriggerLose();
            }
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        // Format time into Minutes and Seconds
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        // Updates UI text (e.g., 05:00)
        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

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