using System.Collections;
using UnityEngine;

public class CarouselColour : MonoBehaviour
{
    public LuggageColour expectedLuggageColour;
    private GameManager gameManager;

    [Header("Flashing Effect")]
    public int flashCount = 3;
    public float flashDuration = 0.2f;

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
        if (gameManager != null && gameManager.IsGameOver())
            return;

        BagColour bag = other.GetComponent<BagColour>();

        if (bag == null) return;

        if (bag.luggageColour == expectedLuggageColour)
        {
            // CORRECT MATCH
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCorrect();

            // Trigger glow on this carousel
            GlowEffect glow = GetComponent<GlowEffect>();
            if (glow != null)
                glow.PlayGlow();

            Debug.Log("CORRECT! " + bag.luggageColour + " matches " + expectedLuggageColour);

            if (gameManager != null && !gameManager.IsGameOver())
            {
                gameManager.AddScore(1);
            }

            Destroy(other.gameObject);
        }
        else
        {
            Debug.Log(" WRONG! Bag is " + bag.luggageColour + " but carousel expects " + expectedLuggageColour);

            // Get the bag's sprite renderer for flashing effect
            SpriteRenderer sr = other.GetComponent<SpriteRenderer>();

            // Start flashing effect (sound will be handled after we know if it's a high score)
            if (sr != null)
            {
                StartCoroutine(FlashBag(sr, other.gameObject));
            }
            else
            {
                // If no sprite renderer, just stop the bag
                BagMovement bagMove = other.GetComponent<BagMovement>();
                if (bagMove != null)
                {
                    bagMove.enabled = false;
                }

                // Stop all other bags
                StopAllBags();

                // Check if it's a high score and play appropriate sound
                if (gameManager != null && !gameManager.IsGameOver())
                {
                    CheckAndPlayGameOverSound();
                    StartCoroutine(DelayedGameOver());
                }
            }
        }
    }

    IEnumerator FlashBag(SpriteRenderer sr, GameObject bagObject)
    {
        // Store original color
        Color originalColor = sr.color;

        // Stop the bag from moving
        BagMovement bagMove = bagObject.GetComponent<BagMovement>();
        if (bagMove != null)
        {
            bagMove.enabled = false;
        }

        // Stop all other bags
        StopAllBags();

        // Flash the bag red multiple times
        for (int i = 0; i < flashCount; i++)
        {
            // Turn red
            sr.color = Color.red;
            yield return new WaitForSeconds(flashDuration);

            // Back to original color
            sr.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }

        // Check if it's a high score and play appropriate sound
        if (gameManager != null && !gameManager.IsGameOver())
        {
            CheckAndPlayGameOverSound();
            StartCoroutine(DelayedGameOver());
        }
    }

    void CheckAndPlayGameOverSound()
    {
        // Check if this will be a new high score
        bool isNewHighScore = false;
        if (gameManager != null)
        {
            // Get current score from game manager
            int currentScore = GetCurrentScoreFromGameManager();
            int personalBest = PlayerPrefs.GetInt("PersonalBest", 0);

            if (currentScore > personalBest)
            {
                isNewHighScore = true;
            }
        }

        // Play the appropriate sound
        if (AudioManager.Instance != null)
        {
            if (isNewHighScore)
            {
                AudioManager.Instance.PlayNewHighScore();
                Debug.Log("Playing new high score sound");
            }
            else
            {
                AudioManager.Instance.PlayWrongEmergency();
                Debug.Log("Playing wrong sound");
            }
        }
    }

    int GetCurrentScoreFromGameManager()
    {
        // You'll need to add a public property to GameManager to get current score
        if (gameManager != null)
        {
            return gameManager.GetCurrentScore();
        }
        return 0;
    }

    void StopAllBags()
    {
        BagMovement[] allBags = FindObjectsOfType<BagMovement>();
        foreach (BagMovement bag in allBags)
        {
            if (bag != null)
            {
                bag.enabled = false;
            }
        }
        Debug.Log("All bags stopped");
    }

    IEnumerator DelayedGameOver()
    {
        // Wait a moment after flashing before showing game over
        yield return new WaitForSeconds(0.5f);

        // Now trigger game over
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }
}