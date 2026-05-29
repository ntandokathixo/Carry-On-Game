using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("In-Game UI")]
    public Text inGameScoreText;
    public Text inGameBestText;
    private string personalBestKey;

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

    [Header("Lives System")]
    public int currentLives = 3;
    public int maxLives = 3;
    public TextMeshProUGUI livesText;
    public GameObject livesTagPanel;
    public AudioClip lifeLostSound;

    [Header("Session Summary")]
    public GameObject sessionSummaryPanel;
    public TextMeshProUGUI summaryScoreText;
    public TextMeshProUGUI summaryBestText;
    public TextMeshProUGUI summaryLivesText;
    public TextMeshProUGUI summarySwapsText;
    public TextMeshProUGUI summaryStreakText;
    public TextMeshProUGUI summaryBagsText;

    private int currentScore = 0;
    private int personalBest = 0;
    private bool isGameOver = false;
    private bool newHighScoreAchieved = false;
    private bool hasPlayedEndSound = false;
    private string playerName = "Player";
    private int lastMoreBagsScore = 0;
    private CarouselSwapManager swapManager;

    // Session tracking variables
    private int sessionBestScore = 0;
    private int sessionSwapsSurvived = 0;
    private int sessionLongestStreak = 0;
    private int currentStreak = 0;
    private int totalBagsSorted = 0;

    void Start()
    {
        if (PlayerNameManager.Instance != null)
        {
            playerName = PlayerNameManager.Instance.CurrentPlayerName;
        }

        // Create level-specific key for personal best
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        personalBestKey = "PersonalBest_" + sceneName;

        personalBest = PlayerPrefs.GetInt(personalBestKey, 0);
        swapManager = FindObjectOfType<CarouselSwapManager>();

        currentLives = maxLives;
        UpdateLivesUI();

        ResetSessionStats();
        UpdateInGameUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (highScorePanel != null)
            highScorePanel.SetActive(false);

        if (sessionSummaryPanel != null)
            sessionSummaryPanel.SetActive(false);

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
        if (Input.GetKeyDown(KeyCode.R))
        {
            personalBest = 0;
            PlayerPrefs.SetInt(personalBestKey, 0);
            PlayerPrefs.Save();
            UpdateInGameUI();
            Debug.Log("Best score reset to 0 for this level");
        }
    }

    void ResetSessionStats()
    {
        sessionBestScore = 0;
        sessionSwapsSurvived = 0;
        sessionLongestStreak = 0;
        currentStreak = 0;
        totalBagsSorted = 0;
    }

    public void AddScore(int points = 1)
    {
        if (isGameOver) return;

        currentScore += points;
        totalBagsSorted++;

        // Track streak
        currentStreak++;
        if (currentStreak > sessionLongestStreak)
        {
            sessionLongestStreak = currentStreak;
        }

        ShowRandomEncouragement();

        int currentStep = currentScore / 7;
        int lastStep = lastMoreBagsScore / 7;

        if (currentStep > lastStep && currentScore >= 7)
        {
            ShowMoreBagsMessage();
            lastMoreBagsScore = currentScore;
        }

        if (swapManager != null)
        {
            swapManager.CheckForSwap(currentScore);
        }

        SpawnManager spawner = FindObjectOfType<SpawnManager>();
        if (spawner != null)
        {
            spawner.OnScoreIncreased(currentScore);
        }

        if (currentScore > personalBest)
        {
            newHighScoreAchieved = true;
        }

        // Track session best
        if (currentScore > sessionBestScore)
        {
            sessionBestScore = currentScore;
        }

        UpdateInGameUI();
    }

    void ShowRandomEncouragement()
    {
        if (FeedbackMessage.Instance == null) return;

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

    public void LoseLife()
    {
        if (isGameOver) return;

        // Reset streak on mistake
        currentStreak = 0;

        currentLives--;
        UpdateLivesUI();

        if (AudioManager.Instance != null && lifeLostSound != null)
        {
            AudioManager.Instance.PlaySound(lifeLostSound);
        }

        Debug.Log("Life lost. Remaining: " + currentLives);

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = currentLives.ToString();
        }
    }

    public void IncrementSwapsSurvived()
    {
        sessionSwapsSurvived++;
    }

    void UpdateSessionSummary()
    {
        if (sessionSummaryPanel == null) return;

        if (summaryScoreText != null)
            summaryScoreText.text = "Score: " + currentScore;

        if (summaryBestText != null)
        {
            bool isNewRecord = currentScore > personalBest;
            summaryBestText.text = "Best: " + personalBest ;
        }

        if (summaryLivesText != null)
            summaryLivesText.text = "Lives left: " + currentLives;

        if (summarySwapsText != null)
            summarySwapsText.text = "Carousel swaps: " + sessionSwapsSurvived;

        if (summaryStreakText != null)
            summaryStreakText.text = "Longest streak: " + sessionLongestStreak;

        if (summaryBagsText != null)
            summaryBagsText.text = "Bags sorted: " + totalBagsSorted;
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        // Check achievements first
        if (AchievementLog.Instance != null)
        {
            AchievementLog.Instance.CheckAchievements(currentScore, sessionSwapsSurvived, sessionLongestStreak, currentLives);
        }

        // Update summary panel with final stats
        UpdateSessionSummary();

        if (currentScore > personalBest)
        {
            personalBest = currentScore;
            PlayerPrefs.SetInt(personalBestKey, personalBest);
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

        StopGameSystems();
    }

    void ShowHighScoreCelebration()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (sessionSummaryPanel != null)
            sessionSummaryPanel.SetActive(true);

        if (highScorePanel != null)
        {
            highScorePanel.SetActive(true);

            if (highScoreMessageTMP != null)
            {
                highScoreMessageTMP.text = "You're so sharp " + playerName + "! You have a new high score of";
            }

            if (highScoreValueTMP != null)
            {
                highScoreValueTMP.text = currentScore.ToString();
            }
        }

        StopGameSystems();
    }

    void ShowGameOverPanel()
    {
        if (highScorePanel != null)
            highScorePanel.SetActive(false);

        if (sessionSummaryPanel != null)
            sessionSummaryPanel.SetActive(true);

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

    public void OnCarouselSwapOccurred()
    {
        sessionSwapsSurvived++;
    }
}