using System.Collections;
using UnityEngine;

public class CarouselColour : MonoBehaviour
{
    public LuggageColour expectedLuggageColour;
    private LuggageColour originalColour;

    [Header("Visuals")]
    public SpriteRenderer carouselSprite;

    [Header("Flashing Effect")]
    public int flashCount = 3;
    public float flashDuration = 0.2f;

    [Header("Swap Effects")]
    public float bounceHeight = 0.5f;
    public float bounceDuration = 0.3f;
    public float pulseScale = 1.3f;
    public float pulseDuration = 0.2f;

    private GameManager gameManager;
    private bool isSwapped = false;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Coroutine currentBounce;
    private Coroutine currentPulse;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        originalColour = expectedLuggageColour;
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;

        if (carouselSprite == null)
            carouselSprite = GetComponent<SpriteRenderer>();
    }

    public void SwapWith(CarouselColour otherCarousel)
    {
        LuggageColour tempColour = expectedLuggageColour;
        expectedLuggageColour = otherCarousel.expectedLuggageColour;
        otherCarousel.expectedLuggageColour = tempColour;

        Debug.Log(gameObject.name + " swapped colours with " + otherCarousel.name);
    }

    public void StartBounce()
    {
        if (currentBounce != null)
        {
            StopCoroutine(currentBounce);
        }
        currentBounce = StartCoroutine(BounceRoutine());
    }

    public void StartPulse()
    {
        if (currentPulse != null)
        {
            StopCoroutine(currentPulse);
        }
        currentPulse = StartCoroutine(PulseTwiceRoutine());
    }

    IEnumerator BounceRoutine()
    {
        float elapsedTime = 0;
        Vector3 startPos = originalPosition;
        Vector3 endPos = startPos + new Vector3(0, bounceHeight, 0);

        while (elapsedTime < bounceDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / bounceDuration;
            t = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        elapsedTime = 0;

        while (elapsedTime < bounceDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / bounceDuration;
            t = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(endPos, startPos, t);
            yield return null;
        }

        transform.position = startPos;
        currentBounce = null;
    }

    IEnumerator PulseTwiceRoutine()
    {
        for (int pulseCount = 0; pulseCount < 2; pulseCount++)
        {
            float growTime = 0;
            Vector3 startScale = originalScale;
            Vector3 endScale = originalScale * pulseScale;

            while (growTime < pulseDuration)
            {
                growTime += Time.unscaledDeltaTime;
                float t = growTime / pulseDuration;
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            transform.localScale = endScale;

            float shrinkTime = 0;
            while (shrinkTime < pulseDuration)
            {
                shrinkTime += Time.unscaledDeltaTime;
                float t = shrinkTime / pulseDuration;
                transform.localScale = Vector3.Lerp(endScale, startScale, t);
                yield return null;
            }

            transform.localScale = startScale;

            if (pulseCount == 0)
            {
                yield return new WaitForSecondsRealtime(0.15f);
            }
        }

        currentPulse = null;
    }

    public void ResetToOriginal()
    {
        expectedLuggageColour = originalColour;
        isSwapped = false;

        transform.position = originalPosition;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;

        if (currentBounce != null)
        {
            StopCoroutine(currentBounce);
            currentBounce = null;
        }

        if (currentPulse != null)
        {
            StopCoroutine(currentPulse);
            currentPulse = null;
        }

        if (carouselSprite != null)
            carouselSprite.color = Color.white;
    }

    public bool IsSwapped()
    {
        return isSwapped;
    }

    public LuggageColour GetOriginalColour()
    {
        return originalColour;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameManager != null && gameManager.IsGameOver())
            return;

        // Check if other object still exists
        if (other == null || other.gameObject == null)
            return;

        BagColour bag = other.GetComponent<BagColour>();
        if (bag == null) return;

        if (bag.luggageColour == expectedLuggageColour)
        {
            // CORRECT
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCorrect();

            GlowEffect glow = GetComponent<GlowEffect>();
            if (glow != null)
                glow.PlayGlow();

            if (gameManager != null && !gameManager.IsGameOver())
            {
                gameManager.AddScore(1);
            }

            Destroy(other.gameObject);
        }
        else
        {
            // WRONG - Lose a life
            Debug.Log("WRONG! Bag is " + bag.luggageColour + " but carousel expects " + expectedLuggageColour);

            // Flash the bag red before destroying
            SpriteRenderer sr = other.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                StartCoroutine(FlashAndDestroy(sr, other.gameObject));
            }
            else
            {
                // Tell game manager to lose a life
                if (gameManager != null && !gameManager.IsGameOver())
                {
                    gameManager.LoseLife();
                }
                Destroy(other.gameObject);
            }
        }
    }

    IEnumerator FlashAndDestroy(SpriteRenderer sr, GameObject bagObject)
    {
        Color originalColor = sr.color;

        // Flash red 3 times quickly
        for (int i = 0; i < 3; i++)
        {
            if (sr != null)
                sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (sr != null)
                sr.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }

        // Tell game manager to lose a life
        if (gameManager != null && !gameManager.IsGameOver())
        {
            gameManager.LoseLife();
        }

        // Destroy the bag
        if (bagObject != null)
            Destroy(bagObject);
    }
}