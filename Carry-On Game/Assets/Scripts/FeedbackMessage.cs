using UnityEngine;
using TMPro;
using System.Collections;

public class FeedbackMessage : MonoBehaviour
{
    public static FeedbackMessage Instance;

    [Header("Message Settings")]
    public GameObject messagePrefab;
    public float messageDuration = 1.2f;
    public float floatUpSpeed = 30f;

    private Canvas canvas;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        canvas = FindObjectOfType<Canvas>();
    }

    public void ShowMessage(string text, Color color)
    {
        if (messagePrefab == null)
        {
            Debug.LogWarning("Message prefab not assigned!");
            return;
        }

        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;
        }

        GameObject msgObj = Instantiate(messagePrefab, canvas.transform);
        TextMeshProUGUI tmp = msgObj.GetComponent<TextMeshProUGUI>();

        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
            StartCoroutine(AnimateMessage(msgObj, tmp));
        }
        else
        {
            Destroy(msgObj);
        }
    }

    IEnumerator AnimateMessage(GameObject msgObj, TextMeshProUGUI tmp)
    {
        RectTransform rect = msgObj.GetComponent<RectTransform>();
        Vector2 startPos = rect.anchoredPosition;  // Changed to Vector2
        float elapsedTime = 0;
        Color startColor = tmp.color;

        // Random slight horizontal offset
        float xOffset = Random.Range(-50f, 50f);
        rect.anchoredPosition = new Vector2(startPos.x + xOffset, startPos.y);  // Fixed: using Vector2

        while (elapsedTime < messageDuration)
        {
            elapsedTime += Time.deltaTime;

            // Float upward
            float t = elapsedTime / messageDuration;
            float yOffset = floatUpSpeed * t;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, startPos.y + yOffset);  // Fixed: using Vector2

            // Fade out
            Color newColor = startColor;
            newColor.a = Mathf.Lerp(1f, 0f, t);
            tmp.color = newColor;

            yield return null;
        }

        Destroy(msgObj);
    }
}