using UnityEngine;

public class LuggageTracker : MonoBehaviour
{
    public LuggageColour colour;
    public SpawnManager spawnManager;

    void OnDestroy()
    {
        if (spawnManager != null)
        {
            spawnManager.OnBagDestroyed(colour);
        }
    }
}