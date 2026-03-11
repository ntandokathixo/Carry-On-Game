using System.Collections;
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
        if (gameManager != null && gameManager.IsGameOver())
            return;

        BagColour bag = other.GetComponent<BagColour>();

        if (bag == null) return;

        

        if (bag.luggageColour == expectedLuggageColour)
        {
            // CORRECT MATCH
            
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayCorrect();
            }

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
            if (gameManager != null && !gameManager.HasPlayedEndSound())
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayWrongEmergency();
            }

            Debug.Log(" WRONG! Bag is " + bag.luggageColour + " but carousel expects " + expectedLuggageColour);

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