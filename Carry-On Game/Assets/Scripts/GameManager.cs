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
    }

    public void AddScore(int points = 1)
    {
        if (isGameOver) return;

        currentScore += points;

        // Check for new personal best
        if (currentScore > personalBest)
        {
            personalBest = currentScore;
            PlayerPrefs.SetInt("PersonalBest", personalBest);
            Debug.Log("New personal best: " + personalBest);
        }

        UpdateUI();
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("GAME OVER! Final Score: " + currentScore);

        // Show game over text
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "GAME OVER\nScore: " + currentScore + "\nBest: " + personalBest;
        }

        // Stop spawning new bags
        SpawnManager spawner = FindObjectOfType<SpawnManager>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }

        // Optional: Freeze all bags
        BagMovement[] allBags = FindObjectsOfType<BagMovement>();
        foreach (BagMovement bag in allBags)
        {
            bag.enabled = false;
        }
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
        SceneManager.LoadScene(restartSceneName);
    }

    // Check if game is over (for other scripts to use)
    public bool IsGameOver()
    {
        return isGameOver;
    }
}