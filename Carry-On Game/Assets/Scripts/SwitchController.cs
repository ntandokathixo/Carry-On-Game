using UnityEngine;

public class SwitchController : MonoBehaviour
{

    public bool goRight = true;
    
    void OnMouseDown()
    {
        goRight = !goRight;
        Debug.Log("Switched. Go Right:" + goRight);
    }

    
}
