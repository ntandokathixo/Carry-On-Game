using UnityEngine;

public class JunctionNode : MonoBehaviour
{

    public Transform leftPath;
    public Transform rightPath;
    public SwitchController switchController;
   

    public Transform GetNextPath()
    {
        if (switchController.goRight)
        {
            return rightPath;
        }
        else
        {
            return leftPath;
        }

    }

   
}
