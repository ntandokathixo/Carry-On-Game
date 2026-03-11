using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "SampleScene";

    [Header("UI Buttons")]
    public Button musicButton;
    public Button soundButton;
    public Button startButton;
    public Button quitButton;

    private bool musicEnabled = true;
    private bool soundEnabled = true;
    private Text musicButtonText;
    private Text soundButtonText;
    private AudioManager audioManager;

    void Start()
    {
        // Find the AudioManager (it should be in this scene now)
        audioManager = FindObjectOfType<AudioManager>();

        if (audioManager == null)
        {
            Debug.LogError("No AudioManager found in Main Menu scene! Please add one.");
            return;
        }

        // Load saved preferences
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;

        // Get text components from buttons
        if (musicButton != null)
            musicButtonText = musicButton.GetComponentInChildren<Text>();

        if (soundButton != null)
            soundButtonText = soundButton.GetComponentInChildren<Text>();

        // Apply settings to AudioManager
        audioManager.SetMusicEnabled(musicEnabled);
        audioManager.SetSoundEnabled(soundEnabled);

        // Update button text
        UpdateButtonText();

        // Add listeners to buttons
        if (musicButton != null)
            musicButton.onClick.AddListener(ToggleMusic);

        if (soundButton != null)
            soundButton.onClick.AddListener(ToggleSound);

        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

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
        Debug.Log("Music " + (musicEnabled ? "ON" : "OFF"));
    }

    void ToggleSound()
    {
        soundEnabled = !soundEnabled;
        PlayerPrefs.SetInt("SoundEnabled", soundEnabled ? 1 : 0);
        PlayerPrefs.Save();

        if (audioManager != null)
            audioManager.SetSoundEnabled(soundEnabled);

        UpdateButtonText();
        Debug.Log("Sound " + (soundEnabled ? "ON" : "OFF"));
    }

    void UpdateButtonText()
    {
        if (musicButtonText != null)
            musicButtonText.text = "Music: " + (musicEnabled ? "ON" : "OFF");

        if (soundButtonText != null)
            soundButtonText.text = "Sound: " + (soundEnabled ? "ON" : "OFF");
    }

    public void StartGame()
    {
        Debug.Log("Starting game...");
        SceneManager.LoadScene(gameSceneName);
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