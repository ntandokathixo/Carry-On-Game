using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class AchievementLog : MonoBehaviour
{
    public static AchievementLog Instance;

    [System.Serializable]
    public class Achievement
    {
        public string id;
        public string title;
        public string description;
        public bool unlocked;
    }

    [Header("Achievement List")]
    public List<Achievement> achievements = new List<Achievement>();

    [Header("UI References")]
    public GameObject achievementPanel;
    public Transform achievementContainer;
    public GameObject achievementPrefab;
    public Button closeButton;
    public Button achievementsButton;

    private int currentScore = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        LoadAchievements();

        if (achievementsButton != null)
            achievementsButton.onClick.AddListener(OpenAchievementPanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseAchievementPanel);

        if (achievementPanel != null)
            achievementPanel.SetActive(false);
    }

    private void LoadAchievements()
    {
        foreach (Achievement ach in achievements)
        {
            ach.unlocked = PlayerPrefs.GetInt("Achievement_" + ach.id, 0) == 1;
        }
    }

    private void SaveAchievement(Achievement ach)
    {
        PlayerPrefs.SetInt("Achievement_" + ach.id, ach.unlocked ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void CheckAchievements(int score, int swapsSurvived, int longestStreak, int livesLeft)
    {
        currentScore = score;

        // SCORE-BASED ACHIEVEMENTS
        CheckScoreAchievement(7, "First Steps");
        CheckScoreAchievement(14, "Getting Busy");
        CheckScoreAchievement(21, "Finding Rhythm");
        CheckScoreAchievement(28, "Sharp Mind");
        CheckScoreAchievement(35, "Heavy Traffic");
        CheckScoreAchievement(28, "Carousel Unlocked");

        // SWAP-BASED ACHIEVEMENTS
        if (swapsSurvived >= 1) UnlockAchievement("First Swap Survivor");
        if (swapsSurvived >= 3) UnlockAchievement("Swap Master");

        // STREAK-BASED ACHIEVEMENTS
        if (longestStreak >= 10) UnlockAchievement("Streak Master");

        // PERFECT SESSION
        if (livesLeft == 3) UnlockAchievement("Perfect Session");
    }

    private void CheckScoreAchievement(int targetScore, string achievementId)
    {
        Achievement ach = achievements.Find(a => a.id == achievementId);
        if (ach != null && !ach.unlocked && currentScore >= targetScore)
        {
            ach.unlocked = true;
            SaveAchievement(ach);
            Debug.Log("Achievement unlocked: " + ach.title);
        }
    }

    public void UnlockAchievement(string id)
    {
        Achievement ach = achievements.Find(a => a.id == id);
        if (ach != null && !ach.unlocked)
        {
            ach.unlocked = true;
            SaveAchievement(ach);
            Debug.Log("Achievement unlocked: " + ach.title);
        }
    }

    public void OpenAchievementPanel()
    {
        if (achievementPanel != null)
        {
            RefreshAchievementList();
            achievementPanel.SetActive(true);
        }
    }

    public void CloseAchievementPanel()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }
    }

    private void RefreshAchievementList()
    {
        foreach (Transform child in achievementContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Achievement ach in achievements)
        {
            GameObject entry = Instantiate(achievementPrefab, achievementContainer);

            TextMeshProUGUI titleText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (titleText != null) titleText.text = ach.title;

            TextMeshProUGUI[] allTexts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            if (allTexts.Length >= 2)
            {
                allTexts[1].text = ach.description;
            }

            Image icon = entry.GetComponentInChildren<Image>();
            if (icon != null)
            {
                // Skip the background image if it's the same as entry
                if (icon == entry.GetComponent<Image>())
                {
                    Image[] allImages = entry.GetComponentsInChildren<Image>();
                    if (allImages.Length >= 2) icon = allImages[1];
                }

                icon.color = ach.unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            Image bg = entry.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = ach.unlocked ? new Color(0.15f, 0.2f, 0.15f) : new Color(0.2f, 0.2f, 0.2f);
            }
        }
    }
}