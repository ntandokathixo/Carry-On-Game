using UnityEngine;

public class SwitchController : MonoBehaviour
{
    public bool goRight = true;
    public GameObject rightSpriteObject;  // Drag RightSprite here
    public GameObject leftSpriteObject;    // Drag LeftSprite here

    void Start()
    {
        UpdateSprite();
    }

    void OnMouseDown()
    {
        goRight = !goRight;
        UpdateSprite();
        Debug.Log("Switch toggled. Go Right: " + goRight);
    }

    void UpdateSprite()
    {
        // Enable the correct sprite, disable the other
        rightSpriteObject.SetActive(goRight);
        leftSpriteObject.SetActive(!goRight);
    }
}