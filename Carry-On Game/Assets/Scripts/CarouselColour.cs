using UnityEngine;

public class CarouselColour : MonoBehaviour
{
    // Set this in the Inspector for each carousel
    public Color carouselColour;

    // Reference to GameManager
    private GameManager gameManager;

    void Start()
    {
        // Find the GameManager in the scene
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("No GameManager found in scene!");
        }
    }

    // Called when something enters this carousel's trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it's a bag
        LuggageColour bag = other.GetComponent<LuggageColour>();

        if (bag != null)
        {
            // Compare colours (with a small tolerance)
            if (ColorsAreClose(bag.bagColour, carouselColour))
            {
                // CORRECT! Win condition
                Debug.Log(bag.name + " reached CORRECT carousel!");

                // Add score
                if (gameManager != null)
                {
                    gameManager.AddScore(1);
                }

                // Bag disappears (already happens in BagMovement)
                // We don't destroy here - BagMovement already destroys
            }
            else
            {
                // WRONG! Loss condition
                Debug.LogError(bag.name + " reached WRONG carousel! GAME OVER");

                // Turn the bag red to highlight the mistake
                SpriteRenderer sr = other.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.red;
                }

                // Trigger game over
                if (gameManager != null)
                {
                    gameManager.GameOver();
                }

                // Stop the bag from moving
                BagMovement bagMove = other.GetComponent<BagMovement>();
                if (bagMove != null)
                {
                    bagMove.enabled = false;
                }
            }
        }
    }

    // Helper function to compare colours (ignores small differences)
    bool ColorsAreClose(Color a, Color b)
    {
        float tolerance = 0.1f;
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }
}