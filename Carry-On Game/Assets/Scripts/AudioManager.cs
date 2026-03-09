using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;  // Singleton for easy access

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Sound Effects")]
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip switchSound;
    public AudioClip newHighScoreSound;  // Optional celebration sound
    

    [Header("Background Music")]
    public AudioClip backgroundMusic;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            AudioListener.volume = 2.0f;
            Debug.Log(" AudioManager Instance created: " + gameObject.name);
        }
        else
        {
            Debug.Log(" Duplicate AudioManager destroyed");
            Destroy(gameObject);
        }
    }
    void Start()
    {
        // Load sound preferences
        bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        bool soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;

        // Apply to audio sources
        if (musicSource != null)
        {
            musicSource.mute = !musicEnabled;
            if (backgroundMusic != null && musicEnabled)
            {
                musicSource.clip = backgroundMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        if (sfxSource != null)
        {
            sfxSource.mute = !soundEnabled;
        }

        Debug.Log("AudioManager started - Music: " + musicEnabled + ", Sound: " + soundEnabled);
    }

    public void PlayCorrect()
    {
        if (sfxSource != null && correctSound != null)
            sfxSource.PlayOneShot(correctSound);
    }

    public void PlayWrongEmergency()
    {
        // Create temporary audio source
        GameObject tempGO = new GameObject("TempWrongSound");
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        tempSource.clip = wrongSound;
        tempSource.volume = 1.0f;
        tempSource.Play();

        // Destroy after sound finishes
        Destroy(tempGO, wrongSound.length);
    }

    public void PlaySwitch()
    {
        if (sfxSource != null && switchSound != null)
            sfxSource.PlayOneShot(switchSound);
    }

    public void PlayNewHighScore()
    {
        if (sfxSource != null && newHighScoreSound != null)
            sfxSource.PlayOneShot(newHighScoreSound);
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = volume;
    }
}