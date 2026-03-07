using UnityEngine;

public class LuggageColour : MonoBehaviour
{
    // Set this in the Inspector for each bag prefab
    public Color bagColour;

    void Start()
    {
        // If not set in Inspector, try to get from sprite
        if (bagColour == Color.clear)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                bagColour = sr.color;
            }
        }
    }
}