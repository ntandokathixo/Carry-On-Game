using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button openNamePopupButton;
    public GameObject nameEntryPopup;
    public TMP_InputField nameInputField;
    public Button saveNameButton;
    public Button cancelNameButton;
    public TextMeshProUGUI currentNameDisplay;

    [Header("Settings")]
    public string defaultName = "Player";
    public int maxNameLength = 15;

    public static PlayerNameManager Instance { get; private set; }
    public string CurrentPlayerName { get; private set; }

    void Awake()
    {
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
        if (nameEntryPopup != null)
            nameEntryPopup.SetActive(false);

        if (openNamePopupButton != null)
            openNamePopupButton.onClick.AddListener(OpenNamePopup);

        if (nameInputField != null)
        {
            nameInputField.text = CurrentPlayerName;
            nameInputField.characterLimit = maxNameLength;
        }

        if (saveNameButton != null)
            saveNameButton.onClick.AddListener(SaveAndClosePopup);

        if (cancelNameButton != null)
            cancelNameButton.onClick.AddListener(CloseNamePopup);

        UpdateDisplayText();
    }

    void OpenNamePopup()
    {
        if (nameEntryPopup != null)
        {
            if (nameInputField != null)
                nameInputField.text = CurrentPlayerName;

            RectTransform rect = nameEntryPopup.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
            }

            nameEntryPopup.transform.SetAsLastSibling();
            nameEntryPopup.SetActive(true);
            Canvas.ForceUpdateCanvases();

            if (nameInputField != null)
            {
                nameInputField.Select();
                nameInputField.ActivateInputField();
            }

            Debug.Log("Name entry popup opened");
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

            if (CurrentPlayerName.Length > maxNameLength)
                CurrentPlayerName = CurrentPlayerName.Substring(0, maxNameLength);

            PlayerPrefs.SetString("PlayerName", CurrentPlayerName);
            PlayerPrefs.Save();

            Debug.Log("Player name saved: " + CurrentPlayerName);
            UpdateDisplayText();
        }
        else
        {
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