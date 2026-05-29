using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelStarter : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject infoPanel;
    public TextMeshProUGUI infoText;
    public Button gotItButton;

    [Header("Slide Settings")]
    public float slideDuration = 0.5f;
    public Vector2 slideFromDirection = new Vector2(-1, 0); // -1 = from left, 1 = from right

    private SpawnManagerExtended spawnManager;
    private RectTransform panelRect;
    private Vector2 startPosition;
    private Vector2 targetPosition;

    void Start()
    {
        spawnManager = FindObjectOfType<SpawnManagerExtended>();

        if (spawnManager != null)
        {
            spawnManager.StopSpawning();
            Debug.Log("SpawnManager found and stopped");
        }

        // Setup sliding panel
        if (infoPanel != null)
        {
            panelRect = infoPanel.GetComponent<RectTransform>();
            targetPosition = panelRect.anchoredPosition;

            float screenWidth = Screen.width;
            float offset = slideFromDirection.x * screenWidth;
            startPosition = targetPosition + new Vector2(offset, 0);

            // Set panel to start position (off-screen)
            panelRect.anchoredPosition = startPosition;

            // Set text
            if (infoText != null)
            {
                infoText.text = "Boss: We've added more carousels. And watch the polka dots. Solid bags go to solid carousels. Dots go to dots. You've got this. Ready?";
            }

            // Setup button
            if (gotItButton != null)
            {
                gotItButton.onClick.AddListener(OnGotItPressed);
            }

            // Show panel and slide in
            infoPanel.SetActive(true);
            StartCoroutine(SlideInPanel());
        }
    }

    IEnumerator SlideInPanel()
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

    IEnumerator SlideOutPanel()
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
        infoPanel.SetActive(false);
    }

    void OnGotItPressed()
    {
        Debug.Log("Got It button pressed");
        StartCoroutine(SlideOutAndStart());
    }

    IEnumerator SlideOutAndStart()
    {
        if (infoPanel != null)
        {
            yield return StartCoroutine(SlideOutPanel());
        }

        if (spawnManager != null)
        {
            spawnManager.StartSpawning();
            Debug.Log("StartSpawning called");
        }

        Destroy(gameObject);
    }
}