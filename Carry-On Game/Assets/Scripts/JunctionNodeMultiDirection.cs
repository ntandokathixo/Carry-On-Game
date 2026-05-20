using UnityEngine;

public class JunctionNodeMultiDirection : MonoBehaviour
{
    [Header("Direction Settings")]
    public int currentDirection = 0; // 0 = left, 1 = straight, 2 = right (or up)

    [Header("Path Transforms")]
    public Transform leftPath;
    public Transform straightPath;  // NEW
    public Transform rightPath;

    [Header("Visuals")]
    public GameObject leftIndicator;
    public GameObject straightIndicator;
    public GameObject rightIndicator;

    private SwitchControllerMultiDirection switchController;

    void Start()
    {
        switchController = GetComponent<SwitchControllerMultiDirection>();
        UpdateIndicators();
    }

    public Transform GetNextPath()
    {
        switch (currentDirection)
        {
            case 0: return leftPath;
            case 1: return straightPath;
            case 2: return rightPath;
            default: return null;
        }
    }

    public void CycleDirection()
    {
        // Cycle through 0,1,2 (left, straight, right)
        currentDirection = (currentDirection + 1) % 3;
        UpdateIndicators();
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