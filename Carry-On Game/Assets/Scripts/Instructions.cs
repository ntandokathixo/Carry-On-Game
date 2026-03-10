using UnityEngine;
using System.Collections;

public class InstructionsManager : MonoBehaviour
{
    public GameObject instructionsPanel;
    public float displayDuration = 5f;
    public float startDelay = 1f;

    public GameObject junctionToGlow; // Changed from JunctionGlow to GameObject
    private JunctionGlow junctionGlowScript; // We'll get the script from this

    void Start()
    {
        if (instructionsPanel != null)
        {
            StartCoroutine(ShowInstructions());
        }
    }

    IEnumerator ShowInstructions()
    {
        yield return new WaitForSeconds(startDelay);

        instructionsPanel.SetActive(true);

        // Get the JunctionGlow script from the GameObject
        if (junctionToGlow != null)
        {
            junctionGlowScript = junctionToGlow.GetComponent<JunctionGlow>();

            if (junctionGlowScript != null)
            {
                junctionGlowScript.StartGlow();
                Debug.Log("Glowing junction: " + junctionToGlow.name);
            }
            else
            {
                Debug.LogError("Junction " + junctionToGlow.name + " has no JunctionGlow script attached!");
            }
        }
        else
        {
            Debug.LogError("No junction assigned to glow!");
        }

        yield return new WaitForSeconds(displayDuration);

        instructionsPanel.SetActive(false);

        if (junctionGlowScript != null)
        {
            junctionGlowScript.StopGlow();
        }
    }

    public void HideInstructions()
    {
        StopAllCoroutines();

        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);

        if (junctionGlowScript != null)
        {
            junctionGlowScript.StopGlow();
        }
    }
}