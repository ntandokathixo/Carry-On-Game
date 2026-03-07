using UnityEngine;

public class BagMovement : MonoBehaviour
{
    public float speed = 0.7f;
    public Transform currentTarget;  // Remove spawnPoint, keep currentTarget

    void Start()
    {
        // Don't set currentTarget here anymore - SpawnManager will set it
    }

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
                // Reached a carousel
                currentTarget = null;
            }
        }
    }
}