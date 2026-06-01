using System.Collections;
using UnityEngine;

public class CarouselColourExtended : CarouselColour
{
    [Header("Extended Settings")]
    public BagType acceptedBagType; // Solid or PolkaDot

    // Remove everything else - parent already has blackBagReward, etc.

    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameManager != null && gameManager.IsGameOver())
            return;

        if (other == null || other.gameObject == null)
            return;

        // Check for black bag first
        BlackBagMarker blackBag = other.GetComponent<BlackBagMarker>();
        if (blackBag != null && blackBag.gameManager != null)
        {
            int livesToAdd = blackBagReward; // This comes from parent class
            blackBag.gameManager.AddLives(livesToAdd);
            blackBag.gameManager.ShowLifeGainPopup(livesToAdd, transform.position);

            Destroy(other.gameObject);

            SpawnManager spawner = FindObjectOfType<SpawnManager>();
            if (spawner != null)
            {
                spawner.EnableSpawning();
            }

            blackBag.gameManager.isBlackBagEventActive = false;

            return;
        }

        // Try to get extended bag component first
        BagColourExtended bagExtended = other.GetComponent<BagColourExtended>();

        if (bagExtended != null)
        {
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