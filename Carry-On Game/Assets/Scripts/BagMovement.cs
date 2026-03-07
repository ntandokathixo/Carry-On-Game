using System.Security.Cryptography;
using UnityEngine;

public class BagMovement : MonoBehaviour
{

    public float speed = 6f;
    public Transform currentTarget;
    public Transform spawnPoint;


    void Start()
    {
        currentTarget = spawnPoint;
    }
    void Update()
    {
        Move();
    }

    void Move()
    {
        if (currentTarget == null) return;

        transform.position = Vector2.MoveTowards
           (transform.position, currentTarget.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, currentTarget.position) < 0.05f)
        {
            transform.position = currentTarget.position;

            JunctionNode junction = currentTarget.GetComponent<JunctionNode>();

            if (junction != null)
            {
                currentTarget = junction.GetNextPath();
            }

            else
            {
                Debug.Log("arrived at store!");
                currentTarget = null;
            }
        }
    }

}