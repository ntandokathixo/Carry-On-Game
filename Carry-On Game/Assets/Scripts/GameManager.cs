using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public Text scoreText;
    public Text gameOverText;
    public Text personalBestText;

    [Header("Game Settings")]
    public string restartSceneName = "SampleScene"; // Change to your scene name

    private int currentScore = 0;
    private int personalBest = 0;
    private bool isGameOver = false;

    void Start()
    {
        // Load personal best from previous sessions
        personalBest = PlayerPrefs.GetInt("PersonalBest", 0);

        // Update UI
        UpdateUI();

        // Hide game over text at start
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
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
            Debug.Log("New personal best: " + personalBest);
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

        // Show game over text
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "GAME OVER\nScore: " + currentScore + "\nBest: " + personalBest;
            Debug.Log("Game over text should now be visible");
        }
        else
        {
            Debug.LogError("GameOverText is not assigned in GameManager!");
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

        // Also find and stop any bags that might not have BagMovement (just in case)
        // This is optional but thorough
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
        SceneManager.LoadScene(restartSceneName);
    }

    // Check if game is over (for other scripts to use)
    public bool IsGameOver()
    {
        return isGameOver;
    }
}