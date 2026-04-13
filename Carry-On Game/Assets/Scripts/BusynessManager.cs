using UnityEngine;
using UnityEngine.UI;

public class BusynessMeter : MonoBehaviour
{
    [Header("References")]
    public SpawnManager spawnManager;
    public Image fillBar;

    [Header("Colors")]
    public Color slowColor = Color.green;
    public Color mediumColor = Color.yellow;
    public Color fastColor = new Color(1f, 0.5f, 0f);
    public Color maxColor = Color.red;

    [Header("Settings")]
    public int bagsForMaxBusyness = 35; // At 35 bags spawned, bar is full

    private float targetFill = 0f;
    private float currentFill = 0f;
    public float smoothSpeed = 5f;
    private int lastBagCount = 0;

    void Start()
    {
        if (spawnManager == null)
            spawnManager = FindObjectOfType<SpawnManager>();

        if (fillBar != null)
            fillBar.fillAmount = 0f;
    }

    void Update()
    {
        if (spawnManager == null) return;

        int totalBags = spawnManager.GetTotalBagsSpawned();

        // Only update when bag count changes
        if (totalBags != lastBagCount)
        {
            lastBagCount = totalBags;
            targetFill = Mathf.Clamp01((float)totalBags / bagsForMaxBusyness);
        }

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

    // Called by SpawnManager when a bag spawns
    public void OnBagSpawned(int totalBags)
    {
        targetFill = Mathf.Clamp01((float)totalBags / bagsForMaxBusyness);
    }
}