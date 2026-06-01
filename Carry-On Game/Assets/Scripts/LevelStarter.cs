using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelStarter : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject infoPanel;
    public Button gotItButton;
    public TextMeshProUGUI infoText;

    [Header("Slide Settings")]
    public float slideDuration = 0.3f;

    [Header("Level 2 Message")]
    [TextArea(5, 10)]
    public string level2Message = "Heads up [PLAYER_NAME]\n\nThe system has been upgraded.\nWe have more carousels to manage,\nand some bags have polka dots.\nStay focused and watch where each bag needs to go!";

    [Header("Level 3 Message")]
    [TextArea(5, 10)]
    public string level3Message = "[PLAYER_NAME]\n\nWe've reached maximum carousel capacity.\nMore bags to track, more dots to match.\nStay focused. You've got this.\nReady?";

    private SpawnManagerExtended spawnManager;
    private RectTransform panelRect;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private string playerName = "Player";

    void Start()
    {
        // Get player name
        if (PlayerNameManager.Instance != null)
        {
            playerName = PlayerNameManager.Instance.CurrentPlayerName;
        }

        // Find spawn manager
        spawnManager = FindObjectOfType<SpawnManagerExtended>();
        if (spawnManager != null)
        {
            spawnManager.StopSpawning();
        }

        // Setup panel sliding
        if (infoPanel != null)
        {
            panelRect = infoPanel.GetComponent<RectTransform>();
            targetPosition = panelRect.anchoredPosition;
            startPosition = new Vector2(-Screen.width, targetPosition.y);
            panelRect.anchoredPosition = startPosition;
        }

        // Set the message based on scene name
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string finalMessage = "";

        if (sceneName == "Level3" || sceneName == "Scene3")
        {
            finalMessage = level3Message.Replace("[PLAYER_NAME]", playerName);
        }
        else
        {
            finalMessage = level2Message.Replace("[PLAYER_NAME]", playerName);
        }

        if (infoText != null)
        {
            infoText.text = finalMessage;
        }

        // Setup button
        if (gotItButton != null)
        {
            gotItButton.onClick.RemoveAllListeners();
            gotItButton.onClick.AddListener(OnButtonPressed);
            gotItButton.interactable = true;
        }

        // Show panel and slide in
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
            StartCoroutine(SlideIn());
        }
    }

    IEnumerator SlideIn()
    {
        float elapsedTime = 0;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideDuration;
            t = Mathf.SmoothStep(0, 1, t);
            panelRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        panelRect.anchoredPosition = targetPosition;
    }

    IEnumerator SlideOut()
    {
        float elapsedTime = 0;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideDuration;
            t = Mathf.SmoothStep(0, 1, t);
            panelRect.anchoredPosition = Vector2.Lerp(targetPosition, startPosition, t);
            yield return null;
        }

        panelRect.anchoredPosition = startPosition;
    }

    void OnButtonPressed()
    {
        StartCoroutine(CloseAndStart());
    }

    IEnumerator CloseAndStart()
    {
        if (infoPanel != null)
        {
            yield return StartCoroutine(SlideOut());
            infoPanel.SetActive(false);
        }

        if (spawnManager != null)
        {
            spawnManager.StartSpawning();
        }

        Destroy(gameObject);
    }
}