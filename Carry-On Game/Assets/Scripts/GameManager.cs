using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("In-Game UI")]
    public Text inGameScoreText;
    public Text inGameBestText;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text gameOverScoreText;
    public Text gameOverBestText;
    public Text gameOverMessageText;
    public Button restartButton;
    public Button menuButton;

    [Header("High Score Celebration Panel")]
    public GameObject highScorePanel;
    public TextMeshProUGUI highScoreMessageTMP;
    public TextMeshProUGUI highScoreValueTMP;
    public Button highScorePlayAgainButton;
    public Button highScoreMenuButton;

    [Header("Feedback Messages")]
    public string[] encouragingMessages = new string[]
    {
        "Nice!",
        "Good!",
        "Keep it up!",
        "Sharp!",
        "Focus!",
        "Great!"
    };

    public string[] moreBagsMessages = new string[]
    {
        "More Bags Coming",
        "Bag Overload Incoming!",
        "It's Getting Busier!",
        "Bags Intensifying!"
    };

    [Header("Carousel Swap")]
    public int firstSwapScore = 42;

    [Header("Game Settings")]
    public string restartSceneName = "SampleScene";
    public string mainMenuSceneName = "MainMenu";

    private int currentScore = 0;
    private int personalBest = 0;
    private bool isGameOver = false;
    private bool newHighScoreAchieved = false;
    private bool hasPlayedEndSound = false;
    private string playerName = "Player";
    private int lastMoreBagsScore = 0;
    private CarouselSwapManager swapManager;

    void Start()
    {
        // Get player name
        if (PlayerNameManager.Instance != null)
        {
            playerName = PlayerNameManager.Instance.CurrentPlayerName;
        }

        // Load personal best
        personalBest = PlayerPrefs.GetInt("PersonalBest", 0);

        // Find swap manager
        swapManager = FindObjectOfType<CarouselSwapManager>();

        // Update UI
        UpdateInGameUI();

        // Hide panels
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
    }

    void Update()
    {
        // Press R to reset best score
        if (Input.GetKeyDown(KeyCode.R))
        {
            personalBest = 0;
            PlayerPrefs.SetInt("PersonalBest", 0);
            PlayerPrefs.Save();
            UpdateInGameUI();
            Debug.Log("Best score reset to 0");
        }
    }

    public void AddScore(int points = 1)
    {
        if (isGameOver) return;

        currentScore += points;

        // Show encouraging message
        ShowRandomEncouragement();

        // Check for more bags (every 7 points)
        int currentStep = currentScore / 7;
        int lastStep = lastMoreBagsScore / 7;

        if (currentStep > lastStep && currentScore >= 7)
        {
            ShowMoreBagsMessage();
            lastMoreBagsScore = currentScore;
        }

        // Check for carousel swap
        if (swapManager != null)
        {
            swapManager.CheckForSwap(currentScore);
        }

        // Notify SpawnManager
        SpawnManager spawner = FindObjectOfType<SpawnManager>();
        if (spawner != null)
        {
            spawner.OnScoreIncreased(currentScore);
        }

        // Check for new personal best
        if (currentScore > personalBest)
        {
            newHighScoreAchieved = true;
        }

        UpdateInGameUI();
    }

    void ShowRandomEncouragement()
    {
        if (FeedbackMessage.Instance == null) return;

        // Show message on scores: 1, 3, 6, 9, 12, 15...
        if (currentScore == 1 || currentScore % 3 == 0)
        {
            if (encouragingMessages.Length > 0)
            {
                int randomIndex = Random.Range(0, encouragingMessages.Length);
                FeedbackMessage.Instance.ShowMessage(encouragingMessages[randomIndex], new Color(0.9f, 0.9f, 0.9f));
            }
        }
    }

    void ShowMoreBagsMessage()
    {
        if (FeedbackMessage.Instance == null) return;

        if (moreBagsMessages.Length > 0)
        {
            int randomIndex = Random.Range(0, moreBagsMessages.Length);
            FeedbackMessage.Instance.ShowMessage(moreBagsMessages[randomIndex], new Color(1f, 0.75f, 0.2f));
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        if (newHighScoreAchieved)
        {
            personalBest = currentScore;
            PlayerPrefs.SetInt("PersonalBest", personalBest);
            PlayerPrefs.Save();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayNewHighScore();
            hasPlayedEndSound = true;

            ShowHighScoreCelebration();
        }
        else
        {
            ShowGameOverPanel();
        }
    }

    void ShowHighScoreCelebration()
    {
        Debug.Log("=== SHOW HIGH SCORE CELEBRATION ===");
        Debug.Log("Player name: " + playerName);
        Debug.Log("Current score: " + currentScore);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (highScorePanel != null)
        {
            highScorePanel.SetActive(true);

            string celebrationMessage = "You're so sharp " + playerName + "! You have a new high score of";

            if (highScoreMessageTMP != null)
            {
                highScoreMessageTMP.text = celebrationMessage;
            }

            // Update the value text with the actual score
            if (highScoreValueTMP != null)
            {
                highScoreValueTMP.text = currentScore.ToString();
                Debug.Log("Set value text to: " + currentScore);
            }
            else
            {
                Debug.LogError("highScoreValueTMP is not assigned!");
            }
        }

        StopGameSystems();
    }

    void ShowGameOverPanel()
    {
        if (highScorePanel != null)
            highScorePanel.SetActive(false);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverScoreText != null)
                gameOverScoreText.text = "Score: " + currentScore;

            if (gameOverBestText != null)
                gameOverBestText.text = "Best: " + personalBest;

            if (gameOverMessageText != null)
                gameOverMessageText.text = "Well Done " + playerName + "!";
        }

        StopGameSystems();
    }

    void StopGameSystems()
    {
        SpawnManager spawner = FindObjectOfType<SpawnManager>();
        if (spawner != null)
            spawner.StopSpawning();

        BagMovement[] allBags = FindObjectsOfType<BagMovement>();
        foreach (BagMovement bag in allBags)
        {
            if (bag != null)
                bag.enabled = false;
        }
    }

    void UpdateInGameUI()
    {
        if (inGameScoreText != null)
            inGameScoreText.text = "Score: " + currentScore;

        if (inGameBestText != null)
            inGameBestText.text = "Best: " + personalBest;
    }

    public void RestartGame()
    {
        // Reset carousels before reloading
        if (swapManager != null)
        {
            swapManager.ResetCarousels();
        }

        SceneManager.LoadScene(restartSceneName);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public bool HasPlayedEndSound()
    {
        return hasPlayedEndSound;
    }
}