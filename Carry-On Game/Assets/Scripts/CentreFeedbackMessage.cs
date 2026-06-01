using UnityEngine;
using TMPro;
using System.Collections;

public class CenterMessage : MonoBehaviour
{
    public static CenterMessage Instance;

    [Header("Message Settings")]
    public GameObject messagePrefab;
    public float messageDuration = 2.5f; 

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
            tmp.fontSize = 36;  // Smaller font
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;

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
        Vector2 startPos = rect.anchoredPosition;
        float elapsedTime = 0;
        Color startColor = tmp.color;

        // Center on screen
        rect.anchoredPosition = new Vector2(0, 0);

        while (elapsedTime < messageDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / messageDuration;
            float yOffset = 30f * t;  // Slower float up
            rect.anchoredPosition = new Vector2(0, startPos.y + yOffset);

            Color newColor = startColor;
            newColor.a = Mathf.Lerp(1f, 0f, t);
            tmp.color = newColor;

            yield return null;
        }

        Destroy(msgObj);
    }
}