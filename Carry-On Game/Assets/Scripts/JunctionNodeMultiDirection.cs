using UnityEngine;

public class JunctionNodeMultiDirection : MonoBehaviour
{
    [Header("Path Transforms")]
    public Transform leftPath;
    public Transform straightPath;
    public Transform rightPath;

    private int currentDirection = 0;

    public void SetDirection(int direction)
    {
        currentDirection = direction;
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
}