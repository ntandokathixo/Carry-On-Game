using UnityEngine;
using UnityEngine.UI;

public class BusynessMeter : MonoBehaviour
{
    [Header("References")]
    public SpawnManager spawnManager;
    public GameManager gameManager;
    public Image fillBar;

    [Header("Colors")]
    public Color slowColor = Color.green;
    public Color mediumColor = Color.yellow;
    public Color fastColor = new Color(1f, 0.5f, 0f);
    public Color maxColor = Color.red;

    [Header("Settings")]
    public float baseSpawnInterval = 4f;
    public float minSpawnInterval = 2.8f;

    private float targetFill = 0f;
    private float currentFill = 0f;
    public float smoothSpeed = 5f;

    void Start()
    {
        if (spawnManager == null)
            spawnManager = FindObjectOfType<SpawnManager>();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (fillBar != null)
            fillBar.fillAmount = 0f;
    }

    void Update()
    {
        if (spawnManager == null) return;

        // Get current spawn interval directly
        float currentInterval = spawnManager.GetCurrentSpawnInterval();

        // Calculate busyness based on spawn interval
        // 0% = base interval (4.0s), 100% = min interval (2.8s)
        float busyness = 1f - ((currentInterval - minSpawnInterval) / (baseSpawnInterval - minSpawnInterval));
        targetFill = Mathf.Clamp01(busyness);

        // Smooth animation
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);

        if (fillBar != null)
        {
            fillBar.fillAmount = currentFill;

            // Colour based on current fill
            if (currentFill < 0.33f)
                fillBar.color = slowColor;
            else if (currentFill < 0.66f)
                fillBar.color = mediumColor;
            else if (currentFill < 0.9f)
                fillBar.color = fastColor;
            else
                fillBar.color = maxColor;
        }
    }
}