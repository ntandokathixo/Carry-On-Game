using UnityEngine;

public class DebugTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("DEBUG TRIGGER: " + other.gameObject.name + " hit " + gameObject.name);
    }
}
