using UnityEngine;

public class LuggageTracker : MonoBehaviour
{
    public LuggageColour colour;
    public MonoBehaviour spawnManager; // Changed to MonoBehaviour to accept both types

    void OnDestroy()
    {
        if (spawnManager == null) return;

        // Try as SpawnManagerExtended first (Level 2/3)
        SpawnManagerExtended extendedManager = spawnManager as SpawnManagerExtended;
        if (extendedManager != null)
        {
            extendedManager.OnBagDestroyed(colour);
            return;
        }

        // Try as original SpawnManager (Level 1)
        SpawnManager originalManager = spawnManager as SpawnManager;
        if (originalManager != null)
        {
            originalManager.OnBagDestroyed(colour);
        }
    }
}