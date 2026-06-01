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

    [Header("Life Regain System")]
    public int livesLostThisSession = 0;
    public int streakForLifeRegain = 15;
    public bool hasSeenLifeRegainNotification = false;
    public GameObject lifeRegainNotificationPanel;
    public TextMeshProUGUI lifeRegainNotificationText;
    public Button lifeRegainNotificationButton;
    public GameObject blackBagPrefab;
    public bool isBlackBagEventActive = false;
    public float panelSlideDuration = 0.3f;

    [Header("Special Bag")]
    public string specialBagName = "Lucky Luggage";

    private int currentScore = 0;
    private int personalBest = 0;
    private bool isGameOver = false;
    private bool newHighScoreAchieved = false;
    private bool hasPlayedEndSound = false;
    private string playerName = "Player";
    private int lastMoreBagsScore = 0;
    private CarouselSwapManager swapManager;
    private RectTransform notificationRect;
    private Vector2 notificationStartPos;
    private Vector2 notificationTargetPos;

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

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        personalBestKey = "PersonalBest_" + sceneName;

        personalBest = PlayerPrefs.GetInt(personalBestKey, 0);
        swapManager = FindObjectOfType<CarouselSwapManager>();

        currentLives = maxLives;
        UpdateLivesUI();

        ResetSessionStats();
        UpdateInGameUI();

       // hasSeenLifeRegainNotification = PlayerPrefs.GetInt("LifeRegainNotif_" + sceneName, 0) == 1;

        if (lifeRegainNotificationPanel != null)
        {
            notificationRect = lifeRegainNotificationPanel.GetComponent<RectTransform>();
            notificationTargetPos = notificationRect.anchoredPosition;
            notificationStartPos = new Vector2(-Screen.width, notificationTargetPos.y);
            notificationRect.anchoredPosition = notificationStartPos;
            lifeRegainNotificationPanel.SetActive(false);
        }

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

        if (Input.GetKeyDown(KeyCode.N))
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            PlayerPrefs.SetInt("LifeRegainNotif_" + sceneName, 0);
            PlayerPrefs.Save();
            hasSeenLifeRegainNotification = false;
            Debug.Log("Life regain notification reset for this level");
        }
    }

    void ResetSessionStats()
    {
        sessionBestScore = 0;
        sessionSwapsSurvived = 0;
        sessionLongestStreak = 0;
        currentStreak = 0;
        totalBagsSorted = 0;
        livesLostThisSession = 0;
        isBlackBagEventActive = false;
    }

    public void AddScore(int points = 1)
    {
        if (isGameOver) return;

        currentScore += points;
        totalBagsSorted++;

        currentStreak++;
        if (currentStreak > sessionLongestStreak)
        {
            sessionLongestStreak = currentStreak;
        }

        if (livesLostThisSession >= 2 && !isBlackBagEventActive && !isGameOver)
        {
            if (currentStreak >= streakForLifeRegain)
            {
                TriggerBlackBagEvent();
            }
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

        currentStreak = 0;

        currentLives--;
        livesLostThisSession++;
        UpdateLivesUI();

        if (AudioManager.Instance != null && lifeLostSound != null)
        {
            AudioManager.Instance.PlaySound(lifeLostSound);
        }

        Debug.Log($"Life lost. Remaining: {currentLives}. Total lost this session: {livesLostThisSession}");

        if (livesLostThisSession == 2 && !isGameOver)
        {
            StartCoroutine(ShowLifeRegainNotification());
        }

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    IEnumerator ShowLifeRegainNotification()
    {
        //hasSeenLifeRegainNotification = true;

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        //PlayerPrefs.SetInt("LifeRegainNotif_" + sceneName, 1);
        //PlayerPrefs.Save();

        Time.timeScale = 0f;

        if (lifeRegainNotificationPanel != null)
        {
            notificationRect.anchoredPosition = notificationStartPos;
            lifeRegainNotificationPanel.SetActive(true);

            if (lifeRegainNotificationText != null)
            {
                lifeRegainNotificationText.text = "Oops. Looks like things are getting out of control.\n\nGet 15 correct bags in a row to earn a black bag.\nGuide it to any carousel to regain lives!";
            }

            float elapsedTime = 0;
            while (elapsedTime < panelSlideDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = elapsedTime / panelSlideDuration;
                t = Mathf.SmoothStep(0, 1, t);
                notificationRect.anchoredPosition = Vector2.Lerp(notificationStartPos, notificationTargetPos, t);
                yield return null;
            }
            notificationRect.anchoredPosition = notificationTargetPos;

            if (lifeRegainNotificationButton != null)
            {
                lifeRegainNotificationButton.onClick.RemoveAllListeners();
                lifeRegainNotificationButton.onClick.AddListener(CloseLifeRegainNotification);
            }
        }
    }

    public void ShowSpecialBagNotification()
    {
        if (CenterMessage.Instance != null)
        {
            string message = $" {specialBagName} INCOMING!";
            Color specialColor = new Color(1f, 0.9f, 0.2f);
            CenterMessage.Instance.ShowMessage(message, specialColor);
        }
    }

    void CloseLifeRegainNotification()
    {
        StartCoroutine(SlideOutAndResume());
    }

    IEnumerator SlideOutAndResume()
    {
        if (lifeRegainNotificationPanel != null)
        {
            float elapsedTime = 0;
            while (elapsedTime < panelSlideDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = elapsedTime / panelSlideDuration;
                t = Mathf.SmoothStep(0, 1, t);
                notificationRect.anchoredPosition = Vector2.Lerp(notificationTargetPos, notificationStartPos, t);
                yield return null;
            }
            notificationRect.anchoredPosition = notificationStartPos;
            lifeRegainNotificationPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    void TriggerBlackBagEvent()
    {
        isBlackBagEventActive = true;
        currentStreak = 0;

        Debug.Log("Special bag event triggered!");

        // Show notification
        ShowSpecialBagNotification();

        SpawnManager spawner = FindObjectOfType<SpawnManager>();
        if (spawner != null)
        {
            spawner.StopSpawning();

            BagMovement[] allBags = FindObjectsOfType<BagMovement>();
            foreach (BagMovement bag in allBags)
            {
                if (bag != null)
                {
                    Destroy(bag.gameObject);
                }
            }
        }

        if (blackBagPrefab != null && spawner != null && spawner.spawnPoint != null && spawner.firstJunction != null)
        {
            GameObject specialBag = Instantiate(blackBagPrefab, spawner.spawnPoint.position, Quaternion.identity);
            specialBag.tag = "Bag";

            BlackBagMarker marker = specialBag.AddComponent<BlackBagMarker>();
            marker.gameManager = this;

            BagMovement bagMove = specialBag.GetComponent<BagMovement>();
            if (bagMove != null)
            {
                bagMove.currentTarget = spawner.firstJunction;
            }

            Debug.Log($"{specialBagName} spawned!");
        }
        else
        {
            Debug.LogError($"Cannot spawn {specialBagName} - missing references!");
            if (spawner != null)
            {
                spawner.EnableSpawning();
            }
            isBlackBagEventActive = false;
        }
    }

    public void AddLives(int amount)
    {
        int oldLives = currentLives;
        currentLives = Mathf.Min(maxLives, currentLives + amount);
        UpdateLivesUI();
        UpdateInGameUI();

        Debug.Log($"Gained {amount} life(s)! Lives: {oldLives} -> {currentLives}");
    }

    public void ShowLifeGainPopup(int amount, Vector3 position)
    {
        if (CenterMessage.Instance != null)
        {
            string message = $"+{amount} LIFE";
            Color goldColor = new Color(1f, 0.8f, 0.2f);
            CenterMessage.Instance.ShowMessage(message, goldColor);
        }
        else
        {
            Debug.LogError("CenterMessage.Instance is NULL!");
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
            summaryBestText.text = "Best: " + personalBest;
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
        Time.timeScale = 1f;

        if (swapManager != null)
        {
            swapManager.ResetCarousels();
        }

        SceneManager.LoadScene(restartSceneName);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
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