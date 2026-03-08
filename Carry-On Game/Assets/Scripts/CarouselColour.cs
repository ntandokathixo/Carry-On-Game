using UnityEngine;

public class CarouselColour : MonoBehaviour
{
    public LuggageColour expectedLuggageColour;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("No GameManager found in scene! Please add one.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        BagColour bag = other.GetComponent<BagColour>();

        if (bag == null) return;

        if (bag.luggageColour == expectedLuggageColour)
        {
            // CORRECT MATCH
            Debug.Log("CORRECT! " + bag.luggageColour + " matches " + expectedLuggageColour);

            if (gameManager != null && !gameManager.IsGameOver())
            {
                gameManager.AddScore(1);
            }

            Destroy(other.gameObject);
        }
        else
        {
            // WRONG MATCH - GAME OVER
            Debug.LogError("WRONG! Bag is " + bag.luggageColour + " but carousel expects " + expectedLuggageColour);

            // Turn bag red
            SpriteRenderer sr = other.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.red;
            }

            // Trigger game over
            if (gameManager != null && !gameManager.IsGameOver())
            {
                gameManager.GameOver();
            }

            // Stop this bag from moving
            BagMovement bagMove = other.GetComponent<BagMovement>();
            if (bagMove != null)
            {
                bagMove.enabled = false;
            }
        }
    }
}