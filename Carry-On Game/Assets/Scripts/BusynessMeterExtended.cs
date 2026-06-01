using UnityEngine;
using UnityEngine.UI;

public class BusynessMeterExtended : MonoBehaviour
{
    [Header("UI References")]
    public Image fillBar;

    [Header("Settings")]
    public int maxBagsForFullMeter = 80;
    public float fillSpeed = 2f;

    private SpawnManagerExtended spawnManager;
    private float targetFill = 0f;
    private float currentFill = 0f;

    void Start()
    {
        spawnManager = FindObjectOfType<SpawnManagerExtended>();

        if (spawnManager == null)
        {
            Debug.LogError("BusynessMeterExtended: No SpawnManagerExtended found!");
        }

        if (fillBar == null)
        {
            Debug.LogError("BusynessMeterExtended: Fill bar not assigned!");
        }

        if (fillBar != null)
        {
            fillBar.fillAmount = 0;
            fillBar.color = Color.green;
        }
    }

    void Update()
    {
        if (spawnManager == null || fillBar == null) return;

        int totalBags = spawnManager.GetTotalBagsSpawned();

        targetFill = Mathf.Clamp01((float)totalBags / maxBagsForFullMeter);
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * fillSpeed);

        fillBar.fillAmount = currentFill;

        if (currentFill < 0.33f)
            fillBar.color = Color.green;
        else if (currentFill < 0.66f)
            fillBar.color = Color.yellow;
        else if (currentFill < 0.9f)
            fillBar.color = new Color(1f, 0.5f, 0f);
        else
            fillBar.color = Color.red;
    }
}