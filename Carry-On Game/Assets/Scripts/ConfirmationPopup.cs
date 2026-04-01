using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmationPopup : MonoBehaviour
{
    [Header("Text Elements (Use either)")]
    public TextMeshProUGUI messageTextTMP;  // For TMP
    public Text messageTextLegacy;           // For regular Text

    public TextMeshProUGUI confirmButtonTextTMP;
    public Text confirmButtonTextLegacy;

    public TextMeshProUGUI cancelButtonTextTMP;
    public Text cancelButtonTextLegacy;

    [Header("Buttons")]
    public Button confirmButton;
    public Button cancelButton;

    private Action onConfirm;
    private Action onCancel;

    public static ConfirmationPopup Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public void Show(string message, Action onConfirmAction, Action onCancelAction = null)
    {
        // Set message text (try TMP first, then legacy)
        if (messageTextTMP != null)
            messageTextTMP.text = message;
        else if (messageTextLegacy != null)
            messageTextLegacy.text = message;
        else
            Debug.LogError("No message text component assigned!");

        onConfirm = onConfirmAction;
        onCancel = onCancelAction;

        gameObject.SetActive(true);
        Debug.Log("Showing confirmation popup: " + message);
    }

    void OnConfirmClicked()
    {
        gameObject.SetActive(false);
        onConfirm?.Invoke();
    }

    void OnCancelClicked()
    {
        gameObject.SetActive(false);
        onCancel?.Invoke();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // Optional: Set button text
    public void SetButtonTexts(string confirmText, string cancelText)
    {
        if (confirmButtonTextTMP != null)
            confirmButtonTextTMP.text = confirmText;
        else if (confirmButtonTextLegacy != null)
            confirmButtonTextLegacy.text = confirmText;

        if (cancelButtonTextTMP != null)
            cancelButtonTextTMP.text = cancelText;
        else if (cancelButtonTextLegacy != null)
            cancelButtonTextLegacy.text = cancelText;
    }
}