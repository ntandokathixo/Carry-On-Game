using UnityEngine;

public class CarouselColour : MonoBehaviour
{
    public LuggageColour expectedLuggageColour;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        Debug.Log("CAROUSEL START: " + gameObject.name + " expects " + expectedLuggageColour);

        // Check collider
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("NO COLLIDER on " + gameObject.name + "!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError("COLLIDER is NOT a TRIGGER on " + gameObject.name + "!");
        }
        else
        {
            Debug.Log("Collider OK on " + gameObject.name);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("TRIGGER ENTERED: " + other.gameObject.name + " hit " + gameObject.name);

        BagColour bag = other.GetComponent<BagColour>();

        if (bag == null)
        {
            Debug.LogWarning("Object has NO BagColour script: " + other.gameObject.name);
            return;
        }

        Debug.Log("Bag has colour: " + bag.luggageColour);
        Debug.Log("Carousel expects: " + expectedLuggageColour);

        if (bag.luggageColour == expectedLuggageColour)
        {
            Debug.Log("CORRECT! " + bag.luggageColour + " matches " + expectedLuggageColour);

            // Add score
            if (gameManager != null)
            {
                gameManager.AddScore(1);
                Debug.Log("Score added!");
            }

            // Destroy bag
            Destroy(other.gameObject);
            Debug.Log("Bag destroyed");
        }
        else
        {
            Debug.LogError("WRONG! Bag is " + bag.luggageColour + " but carousel expects " + expectedLuggageColour);

            // Turn bag red
            SpriteRenderer sr = other.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.red;
                Debug.Log("Bag turned red");
            }

            // Game over
            if (gameManager != null)
            {
                gameManager.GameOver();
                Debug.Log("Game Over triggered");
            }

            // Stop bag movement
            BagMovement bagMove = other.GetComponent<BagMovement>();
            if (bagMove != null)
            {
                bagMove.enabled = false;
                Debug.Log("Bag movement stopped");
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("TRIGGER STAY: " + other.gameObject.name + " still in " + gameObject.name);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("TRIGGER EXIT: " + other.gameObject.name + " left " + gameObject.name);
    }
}