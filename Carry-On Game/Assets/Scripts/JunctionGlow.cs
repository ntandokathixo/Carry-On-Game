using UnityEngine;
using System.Collections;

public class JunctionGlow : MonoBehaviour
{
    public Color glowColor = new Color(1f, 1f, 0.5f, 1f); // Soft yellow
    public float pulseSpeed = 2f;
    public float glowStrength = 0.7f; // How intense the glow is (0 to 1)

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isGlowing = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void StartGlow()
    {
        if (spriteRenderer == null) return;

        isGlowing = true;
        StopAllCoroutines();
        StartCoroutine(GlowRoutine());
        Debug.Log("Started glow on " + gameObject.name);
    }

    public void StopGlow()
    {
        isGlowing = false;
        StopAllCoroutines();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        Debug.Log("Stopped glow on " + gameObject.name);
    }

    IEnumerator GlowRoutine()
    {
        float time = 0;

        while (isGlowing)
        {
            time += Time.deltaTime * pulseSpeed;
            // Smooth sine wave pulse between 0 and 1
            float t = (Mathf.Sin(time) + 1f) / 2f;
            // Apply glow strength to make it more subtle
            float glowAmount = t * glowStrength;

            // Blend between original color and glow color
            Color targetColor = Color.Lerp(originalColor, glowColor, glowAmount);
            spriteRenderer.color = targetColor;

            yield return null;
        }
    }
}