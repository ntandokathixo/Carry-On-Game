using UnityEngine;

public class SwitchControllerMultiDirection : MonoBehaviour
{
    private JunctionNodeMultiDirection junctionNode;

    void Start()
    {
        junctionNode = GetComponent<JunctionNodeMultiDirection>();
    }

    void OnMouseDown()
    {
        if (junctionNode != null)
        {
            junctionNode.CycleDirection();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySwitch();

            Debug.Log("Switch cycled. Direction: " + junctionNode.currentDirection);
        }
    }
}