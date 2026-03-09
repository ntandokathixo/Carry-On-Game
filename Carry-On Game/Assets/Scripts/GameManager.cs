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
    public Text gameOverTitleText;     // "GAME OVER"
    public Text gameOverScoreText;      // dynamic score
    public Text gameOverBestText;       // dynamic best
    public Button restartButton;

    [Header("Game Settings")]
    public string restartSceneName = "SampleScene"; // Change to your scene name

    private int currentScore = 0;
    private int personalBest = 0;
    private bool isGameOver = false;

    [Header("Celebration Effects")]
    public GameObject highScorePanel;
    public float celebrationDuration = 3f;
    private bool newHighScoreAchieved = false;

    void Start()
    {
        // Load personal best from previous sessions
        personalBest = PlayerPrefs.GetInt("PersonalBest", 0);

        // Update UI
        UpdateUI();

        // Hide game over panel at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Set up restart button listener
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        // Hide high score panel at start
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(false);
        }

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

        // Check for new personal best
        if (currentScore > personalBest)
        {
            personalBest = currentScore;
            PlayerPrefs.SetInt("PersonalBest", personalBest);
            PlayerPrefs.Save(); // Force save immediately

            // Play high score sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayNewHighScore();

            // Show celebration (only if not already showing)
            if (!newHighScoreAchieved)
                ShowNewHighScoreCelebration();

            Debug.Log("New personal best: " + personalBest);
        }

        // Notify SpawnManager to increase difficulty based on score
        SpawnManager spawner = FindObjectOfType<SpawnManager>();
        if (spawner != null)
        {
            spawner.OnScoreIncreased(currentScore);
        }

        UpdateUI();
    }

    public void GameOver()
    {
        if (isGameOver)
        {
            Debug.Log("GameOver called but game is already over");
            return;
        }

        isGameOver = true;

        Debug.Log("GAME OVER! Final Score: " + currentScore);

        // STOP BACKGROUND MUSIC
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
            Debug.Log("Background music stopped");
        }

        // SHOW GAME OVER PANEL instead of old text
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverTitleText != null)
                gameOverTitleText.text = "GAME OVER";

            if (gameOverScoreText != null)
                gameOverScoreText.text = "Score: " + currentScore;

            if (gameOverBestText != null)
                gameOverBestText.text = "Best: " + personalBest;

            Debug.Log("Game over panel shown");
        }
        else
        {
            Debug.LogError("GameOverPanel is not assigned in GameManager!");
        }

        // Stop spawning new bags
        SpawnManager spawner = FindObjectOfType<SpawnManager>();
        if (spawner != null)
        {
            spawner.StopSpawning();
            Debug.Log("Spawning stopped");
        }
        else
        {
            Debug.LogError("SpawnManager not found!");
        }

        // Stop ALL bags from moving
        BagMovement[] allBags = FindObjectsOfType<BagMovement>();
        Debug.Log("Found " + allBags.Length + " bags to stop");

        foreach (BagMovement bag in allBags)
        {
            if (bag != null)
            {
                bag.enabled = false;
                Debug.Log("Stopped bag: " + bag.gameObject.name);
            }
        }

        newHighScoreAchieved = false;  // Reset for next game
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }

        if (personalBestText != null)
        {
            personalBestText.text = "Best: " + personalBest;
        }
    }

    // Call this to restart (e.g., from a button)
    public void RestartGame()
    {
        Debug.Log("Restarting game...");

        // Hide panels before reload
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (highScorePanel != null)
            highScorePanel.SetActive(false);

        SceneManager.LoadScene(restartSceneName);
    }

    // Check if game is over (for other scripts to use)
    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void ShowNewHighScoreCelebration()
    {
        newHighScoreAchieved = true;

        // Show panel
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(true);

            // Update score text in panel if you have one
            Text panelText = highScorePanel.GetComponentInChildren<Text>();
            if (panelText != null)
                panelText.text = "NEW HIGH SCORE!\n" + personalBest;
        }

        // Auto-hide after duration
        Invoke("HideCelebration", celebrationDuration);
    }

    void HideCelebration()
    {
        if (highScorePanel != null)
            highScorePanel.SetActive(false);
    }
}