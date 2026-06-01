using UnityEngine;
using UnityEngine.UI;

public class BusynessMeter : MonoBehaviour
{
    [Header("UI References")]
    public Image fillBar;

    [Header("Settings")]
    public int maxBagsForFullMeter = 35;  // At 35 bags spawned, meter is 100% full

    private SpawnManager spawnManager;
    private int lastBagCount = 0;
    private float targetFill = 0f;
    private float currentFill = 0f;
    public float fillSpeed = 2f;  // How fast the bar animates

    void Start()
    {
        spawnManager = FindObjectOfType<SpawnManager>();

        if (spawnManager == null)
        {
            Debug.LogError("BusynessMeter: No SpawnManager found!");
        }

        if (fillBar == null)
        {
            Debug.LogError("BusynessMeter: Fill bar not assigned!");
        }

        // Initialize fill bar
        if (fillBar != null)
        {
            fillBar.fillAmount = 0;
            fillBar.color = Color.green;
        }
    }

    void Update()
    {
        if (spawnManager == null || fillBar == null) return;

        // Get total bags spawned (you need to add this to SpawnManager)
        int totalBags = spawnManager.GetTotalBagsSpawned();

        // Calculate target fill (0 to 1)
        targetFill = Mathf.Clamp01((float)totalBags / maxBagsForFullMeter);

        // Smoothly move towards target
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * fillSpeed);

        // Update fill bar
        fillBar.fillAmount = currentFill;

        // Change color based on fill level
        if (currentFill < 0.33f)
            fillBar.color = Color.green;
        else if (currentFill < 0.66f)
            fillBar.color = Color.yellow;
        else if (currentFill < 0.9f)
            fillBar.color = new Color(1f, 0.5f, 0f); // Orange
        else
            fillBar.color = Color.red;
    }
}