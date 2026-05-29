using UnityEngine;

public class SwitchControllerMultiDirection : MonoBehaviour
{
    [Header("Direction Settings")]
    public int currentDirection = 0; // 0 = left, 1 = straight, 2 = right

    [Header("Sprite Visuals")]
    public GameObject leftIndicator;
    public GameObject straightIndicator;
    public GameObject rightIndicator;

    [Header("Junction Reference")]
    public JunctionNodeMultiDirection junctionNode; // Drag the parent junction here

    void Start()
    {
        if (junctionNode != null)
        {
            junctionNode.SetDirection(currentDirection);
        }
        UpdateIndicators();
    }

    void OnMouseDown()
    {
        if (junctionNode != null)
        {
            currentDirection = (currentDirection + 1) % 3;
            junctionNode.SetDirection(currentDirection);
            UpdateIndicators();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySwitch();

            Debug.Log("Switch cycled. Direction: " + currentDirection);
        }
    }

    void UpdateIndicators()
    {
        if (leftIndicator != null)
            leftIndicator.SetActive(currentDirection == 0);
        if (straightIndicator != null)
            straightIndicator.SetActive(currentDirection == 1);
        if (rightIndicator != null)
            rightIndicator.SetActive(currentDirection == 2);
    }
}