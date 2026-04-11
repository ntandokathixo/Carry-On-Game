using UnityEngine;

public class BagMovement : MonoBehaviour
{
    public float speed = 3f;
    public Transform currentTarget;

    void Update()
    {
        Move();
    }

    void Move()
    {
        if (currentTarget == null) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            currentTarget.position,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, currentTarget.position) < 0.05f)
        {
            transform.position = currentTarget.position;

            // Check if current target is a waypoint
            Waypoint waypoint = currentTarget.GetComponent<Waypoint>();
            if (waypoint != null && waypoint.nextPoint != null)
            {
                currentTarget = waypoint.nextPoint;
                Debug.Log(gameObject.name + " reached waypoint, moving to next: " + currentTarget.name);
                return;
            }

            // Check if current target is a junction
            JunctionNode junction = currentTarget.GetComponent<JunctionNode>();
            if (junction != null)
            {
                Transform nextPath = junction.GetNextPath();

                if (nextPath != null)
                {
                    currentTarget = nextPath;
                }
                else
                {
                    currentTarget = null;
                }
            }
            else
            {
                Debug.Log("BAG REACHED DESTINATION: " + gameObject.name + " at " + currentTarget.name);
                currentTarget = null;
            }
        }
    }
}