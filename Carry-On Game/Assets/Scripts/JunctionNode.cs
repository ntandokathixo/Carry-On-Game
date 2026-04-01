using UnityEngine;

public class JunctionNode : MonoBehaviour
{
    public Transform leftPath;
    public Transform rightPath;
    public SwitchController switchController;

    public Transform GetNextPath()
    {
        Transform nextDestination;

        if (switchController.goRight)
        {
            nextDestination = rightPath;
        }
        else
        {
            nextDestination = leftPath;
        }

        // If the destination is a waypoint, we need to chain through it
        if (nextDestination != null)
        {
            Waypoint waypoint = nextDestination.GetComponent<Waypoint>();
            if (waypoint != null && waypoint.nextPoint != null)
            {
                // Return the waypoint, the bag will go there and then continue
                return nextDestination;
            }
        }

        return nextDestination;
    }
}
