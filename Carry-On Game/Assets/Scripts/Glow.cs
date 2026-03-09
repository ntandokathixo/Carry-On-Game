using UnityEngine;
using System.Collections;

public class GlowEffect : MonoBehaviour
{
    public float glowDuration = 0.5f;
    public Color glowColor = Color.yellow;
    public int glowIntensity = 2;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void PlayGlow()
    {
        if (spriteRenderer != null)
            StartCoroutine(GlowRoutine());
    }

    IEnumerator GlowRoutine()
    {
        float elapsedTime = 0;

        while (elapsedTime < glowDuration)
        {
            // Pulse between original and glow color
            float t = Mathf.PingPong(elapsedTime * 4, 1);
            spriteRenderer.color = Color.Lerp(originalColor, glowColor, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Return to original
        spriteRenderer.color = originalColor;
    }
}