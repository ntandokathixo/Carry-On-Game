using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public Text scoreText;
    public Text personalBestText;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text gameOverMessageText;
    public Button restartButton;
    public Button menuButton;

    [Header("High Score Celebration")]
    public GameObject highScorePanel;  // Will show at Game Over if new record
    public Text highScoreMessageText;
    public Button highScorePlayAgainButton;
    public Button highScoreMenuButton;
    public float celebrationDuration = 3f;

    [Header("Game Settings")]
    public string restartSceneName = "SampleScene";
    public string mainMenuSceneName = "MainMenu";

    private int currentScore = 0;
    private int personalBest = 0;
    private bool isGameOver = false;
    private bool newHighScoreAchieved = false; // Track if this game set a record

    void Start()
    {
        // Load personal best from previous sessions
        personalBest = PlayerPrefs.GetInt("PersonalBest", 0);

        // Update UI
        UpdateUI();

        // Hide panels at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (highScorePanel != null)
            highScorePanel.SetActive(false);

        // Set up button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMainMenu);

        if (highScorePlayAgainButton != null)
            highScorePlayAgainButton.onClick.AddListener(RestartGame);

        if (highScoreMenuButton != null)
            highScoreMenuButton.onClick.AddListener(GoToMainMenu);

        Debug.Log("GameManager started. Current best: " + personalBest);
    }

    

    public void AddScore(int points = 1)
    {
        if (isGameOver)
        {
            Debug.Log("Tried to add score but game is over");
            return;
        }

        currentScore += points;
        Debug.Log("Score added! Current score: " + currentScore);

        // Check for new personal best (just track it, don't show yet)
        if (currentScore > personalBest)
        {
            newHighScoreAchieved = true; // Mark that we beat the record
            Debug.Log("New high score achieved! Will show at Game Over");
        }

        // Notify SpawnManager to increase difficulty
        SpawnManager spawner = FindObjectOfType<SpawnManager>();
        if (spawner != null)
        {
            spawner.OnScoreIncreased(currentScore);
        }

        UpdateUI();
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("GAME OVER! Final Score: " + currentScore + ", Best: " + personalBest);

        // STOP BACKGROUND MUSIC
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        // Check if this was a new high score
        if (newHighScoreAchieved)
        {
            // Save the new personal best
            personalBest = currentScore;
            PlayerPrefs.SetInt("PersonalBest", personalBest);
            PlayerPrefs.Save();

            // Play high score sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayNewHighScore();

            // SHOW HIGH SCORE PANEL FIRST
            if (highScorePanel != null)
            {
                highScorePanel.SetActive(true);
                Debug.Log("High score panel activated");

                // Update high score text
                Text panelText = highScorePanel.GetComponentInChildren<Text>();
                if (panelText != null)
                {
                    panelText.text = "\n" + personalBest;
                }

                
            }
            else
            {
                // If no high score panel, just show game over panel
                Debug.LogError("highScorePanel is NULL!");
                ShowGameOverPanel();
            }
        }
        else
        {
            // No new record, show game over panel immediately
            ShowGameOverPanel();
        }
    }

    void ShowGameOverPanel()
    {
        // Hide high score panel if it was showing
        if (highScorePanel != null)
            highScorePanel.SetActive(false);

        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverMessageText != null)
            {
                gameOverMessageText.text = "\n\nScore: " + currentScore + "\nBest: " + personalBest;
            }

            Debug.Log("Game over panel shown");
        }

        // Stop spawning
        SpawnManager spawner = FindObjectOfType<SpawnManager>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }

        // Stop ALL bags
        BagMovement[] allBags = FindObjectsOfType<BagMovement>();
        foreach (BagMovement bag in allBags)
        {
            if (bag != null)
                bag.enabled = false;
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;

        if (personalBestText != null)
            personalBestText.text = "Best: " + personalBest;
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        SceneManager.LoadScene(restartSceneName);
    }

    public void GoToMainMenu()
    {
        Debug.Log("Going to main menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}