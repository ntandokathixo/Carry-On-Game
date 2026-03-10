using UnityEngine;
using System.Collections;

public class JunctionGlow : MonoBehaviour
{
    public Color glowColor = Color.yellow;
    public float pulseSpeed = 0.2f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine currentGlowRoutine;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError("Junction " + gameObject.name + " has no SpriteRenderer!");
        }
    }

    public void StartGlow()
    {
        if (spriteRenderer == null) return;

        // Stop any existing glow
        if (currentGlowRoutine != null)
        {
            StopCoroutine(currentGlowRoutine);
        }

        // Start new glow
        currentGlowRoutine = StartCoroutine(GlowRoutine());
        Debug.Log("Started glow on " + gameObject.name);
    }

    public void StopGlow()
    {
        if (currentGlowRoutine != null)
        {
            StopCoroutine(currentGlowRoutine);
            currentGlowRoutine = null;
        }

        // Reset to original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        Debug.Log("Stopped glow on " + gameObject.name);
    }

    IEnumerator GlowRoutine()
    {
        while (true)
        {
            // Pulse between original and glow color
            float t = Mathf.PingPong(Time.time * pulseSpeed, 1);
            spriteRenderer.color = Color.Lerp(originalColor, glowColor, t);
            yield return null;
        }
    }
}