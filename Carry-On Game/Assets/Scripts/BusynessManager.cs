using UnityEngine;
using UnityEngine.UI;

public class BusynessMeter : MonoBehaviour
{
    public Slider busynessSlider;
    public SpawnManagerExtended spawnManager;
    public float baseSpawnInterval = 5.5f;
    public float minSpawnInterval = 3.2f;

    void Update()
    {
        if (spawnManager != null)
        {
            float currentInterval = spawnManager.GetCurrentSpawnInterval();
            float busyness = 1f - ((currentInterval - minSpawnInterval) / (baseSpawnInterval - minSpawnInterval));
            busyness = Mathf.Clamp01(busyness);
            busynessSlider.value = busyness;
        }
    }
}