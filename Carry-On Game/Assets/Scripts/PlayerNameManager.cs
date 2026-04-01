using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button openNamePopupButton;      // Button to open the popup
    public GameObject nameEntryPopup;       // The popup panel
    public TMP_InputField nameInputField;   // Input field inside popup
    public Button saveNameButton;           // Save button inside popup
    public Button cancelNameButton;         // Cancel button inside popup
    public TextMeshProUGUI currentNameDisplay; // Optional: display current name on main menu

    [Header("Settings")]
    public string defaultName = "Player";
    public int maxNameLength = 15;

    public static PlayerNameManager Instance { get; private set; }
    public string CurrentPlayerName { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPlayerName();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Ensure popup starts hidden
        if (nameEntryPopup != null)
            nameEntryPopup.SetActive(false);

        // Set up open popup button
        if (openNamePopupButton != null)
        {
            openNamePopupButton.onClick.AddListener(OpenNamePopup);
        }

        // Set up input field
        if (nameInputField != null)
        {
            nameInputField.text = CurrentPlayerName;
            nameInputField.characterLimit = maxNameLength;
        }

        // Set up save button
        if (saveNameButton != null)
        {
            saveNameButton.onClick.AddListener(SaveAndClosePopup);
        }

        // Set up cancel button
        if (cancelNameButton != null)
        {
            cancelNameButton.onClick.AddListener(CloseNamePopup);
        }

        // Update display text if exists
        UpdateDisplayText();
    }

    void OpenNamePopup()
    {
        if (nameEntryPopup != null)
        {
            // Reset input field to current name
            if (nameInputField != null)
                nameInputField.text = CurrentPlayerName;

            nameEntryPopup.SetActive(true);
            Debug.Log("Name entry popup opened");

            // Automatically select the input field
            if (nameInputField != null)
            {
                nameInputField.Select();
                nameInputField.ActivateInputField();
            }
        }
    }

    void CloseNamePopup()
    {
        if (nameEntryPopup != null)
        {
            nameEntryPopup.SetActive(false);
            Debug.Log("Name entry popup closed");
        }
    }

    void SaveAndClosePopup()
    {
        SavePlayerName();
        CloseNamePopup();
    }

    public void SavePlayerName()
    {
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
        {
            CurrentPlayerName = nameInputField.text.Trim();

            // Limit length
            if (CurrentPlayerName.Length > maxNameLength)
                CurrentPlayerName = CurrentPlayerName.Substring(0, maxNameLength);

            // Save to PlayerPrefs
            PlayerPrefs.SetString("PlayerName", CurrentPlayerName);
            PlayerPrefs.Save();

            Debug.Log("Player name saved: " + CurrentPlayerName);

            // Update display
            UpdateDisplayText();
        }
        else
        {
            // If empty, use default name
            CurrentPlayerName = defaultName;
            if (nameInputField != null)
                nameInputField.text = CurrentPlayerName;
            PlayerPrefs.SetString("PlayerName", CurrentPlayerName);
            PlayerPrefs.Save();
        }
    }

    void LoadPlayerName()
    {
        CurrentPlayerName = PlayerPrefs.GetString("PlayerName", defaultName);
        Debug.Log("Loaded player name: " + CurrentPlayerName);
    }

    void UpdateDisplayText()
    {
        if (currentNameDisplay != null)
        {
            currentNameDisplay.text = "Name: " + CurrentPlayerName;
        }
    }

    // Optional: Reset name (for debugging)
    public void ResetPlayerName()
    {
        CurrentPlayerName = defaultName;
        PlayerPrefs.SetString("PlayerName", defaultName);
        PlayerPrefs.Save();

        if (nameInputField != null)
            nameInputField.text = CurrentPlayerName;

        UpdateDisplayText();
        Debug.Log("Player name reset to default");
    }
}