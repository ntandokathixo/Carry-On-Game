using UnityEngine;

public class BagColour : MonoBehaviour
{
    public LuggageColour luggageColour;  // Now shows as dropdown!

    void Start()
    {
        Debug.Log(gameObject.name + " is colour: " + luggageColour);
    }
}