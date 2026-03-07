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
                // Reached a carousel - DESTROY the bag (no recycling visible)
                Debug.Log(gameObject.name + " reached carousel - destroyed");

                // Tell GameManager to add score (will implement next)
                // FindObjectOfType<GameManager>().AddScore();

                Destroy(gameObject);
            }
        }
    }
}