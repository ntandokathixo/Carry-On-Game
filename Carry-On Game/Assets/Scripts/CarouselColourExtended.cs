using System.Collections;
using UnityEngine;

public class CarouselColourExtended : CarouselColour
{
    [Header("Extended Settings")]
    public BagType acceptedBagType; // Solid or PolkaDot

    void OnTriggerEnter2D(Collider2D other)
    {
        // Access gameManager through the parent property
        // Since gameManager is protected, we can still use it in child class
        if (gameManager != null && gameManager.IsGameOver())
            return;

        if (other == null || other.gameObject == null)
            return;

        // Try to get extended bag component first
        BagColourExtended bagExtended = other.GetComponent<BagColourExtended>();

        if (bagExtended != null)
        {
            // Extended check: colour AND type must match
            if (bagExtended.luggageColour == expectedLuggageColour &&
                bagExtended.bagType == acceptedBagType)
            {
                CorrectMatch(other.gameObject);
            }
            else
            {
                WrongMatch(other.gameObject);
            }
        }
        else
        {
            // Fall back to original behaviour (for Level 1 bags if they appear here)
            BagColour bag = other.GetComponent<BagColour>();
            if (bag != null)
            {
                if (bag.luggageColour == expectedLuggageColour)
                {
                    CorrectMatch(other.gameObject);
                }
                else
                {
                    WrongMatch(other.gameObject);
                }
            }
        }
    }

    void CorrectMatch(GameObject bagObject)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCorrect();

        GlowEffect glow = GetComponent<GlowEffect>();
        if (glow != null)
            glow.PlayGlow();

        if (gameManager != null && !gameManager.IsGameOver())
        {
            gameManager.AddScore(1);
        }

        Destroy(bagObject);
    }

    void WrongMatch(GameObject bagObject)
    {
        Debug.Log("WRONG! Bag colour or type mismatch for " + gameObject.name);

        SpriteRenderer sr = bagObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            StartCoroutine(FlashAndDestroy(sr, bagObject));
        }
        else
        {
            if (gameManager != null && !gameManager.IsGameOver())
            {
                gameManager.LoseLife();
            }
            Destroy(bagObject);
        }
    }

    IEnumerator FlashAndDestroy(SpriteRenderer sr, GameObject bagObject)
    {
        Color originalColor = sr.color;

        for (int i = 0; i < 3; i++)
        {
            if (sr != null)
                sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (sr != null)
                sr.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }

        if (gameManager != null && !gameManager.IsGameOver())
        {
            gameManager.LoseLife();
        }

        if (bagObject != null)
            Destroy(bagObject);
    }
}