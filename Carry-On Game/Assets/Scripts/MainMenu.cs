using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    public string carousel1SceneName = "Scene1";  // Carousel 1 (6 carousels, solid bags)
    public string carousel2SceneName = "Scene2";  // Carousel 2 (12 carousels, solid + polka dot)
    public string carousel3SceneName = "Scene3";  // Carousel 3 (15 carousels, solid + polka dot)

    [Header("UI Buttons")]
    public Button musicButton;
    public Button soundButton;
    public Button carousel1Button;
    public Button carousel2Button;
    public Button carousel3Button;
    public Button quitButton;

    private bool musicEnabled = true;
    private bool soundEnabled = true;
    private Text musicButtonText;
    private Text soundButtonText;
    public GameObject soundDisabledLine;
    private AudioManager audioManager;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();

        if (audioManager == null)
        {
            Debug.LogError("No AudioManager found in Main Menu scene!");
            return;
        }

        // Load saved preferences
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;

        // Get text components
        if (musicButton != null)
            musicButtonText = musicButton.GetComponentInChildren<Text>();

        if (soundButton != null)
            soundButtonText = soundButton.GetComponentInChildren<Text>();

        // Apply settings
        audioManager.SetMusicEnabled(musicEnabled);
        audioManager.SetSoundEnabled(soundEnabled);

        // Update button visuals
        UpdateButtonText();
        soundDisabledLine.SetActive(!soundEnabled);

        // Add button listeners
        if (musicButton != null)
            musicButton.onClick.AddListener(ToggleMusic);

        if (soundButton != null)
            soundButton.onClick.AddListener(ToggleSound);

        if (carousel1Button != null)
            carousel1Button.onClick.AddListener(() => StartGame(carousel1SceneName, "Carousel 1"));

        if (carousel2Button != null)
            carousel2Button.onClick.AddListener(() => StartGame(carousel2SceneName, "Carousel 2"));

        if (carousel3Button != null)
            carousel3Button.onClick.AddListener(() => StartGame(carousel3SceneName, "Carousel 3"));

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
        PlayerPrefs.Save();

        if (audioManager != null)
            audioManager.SetMusicEnabled(musicEnabled);

        UpdateButtonText();
    }

    void ToggleSound()
    {
        soundEnabled = !soundEnabled;
        PlayerPrefs.SetInt("SoundEnabled", soundEnabled ? 1 : 0);
        PlayerPrefs.Save();
        soundDisabledLine.SetActive(!soundEnabled);

        if (audioManager != null)
            audioManager.SetSoundEnabled(soundEnabled);

        UpdateButtonText();
    }

    void UpdateButtonText()
    {
        if (musicButtonText != null)
            musicButtonText.text = "Music: " + (musicEnabled ? "ON" : "OFF");

        if (soundButtonText != null)
            soundButtonText.text = "Sound: " + (soundEnabled ? "ON" : "OFF");
    }

    void StartGame(string sceneName, string carouselName)
    {
        Debug.Log($"Starting {carouselName}...");
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}