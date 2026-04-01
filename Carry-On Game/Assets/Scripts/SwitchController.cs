using UnityEngine;

public class SwitchController : MonoBehaviour
{
    public bool goRight = true;
    public GameObject rightSpriteObject;
    public GameObject leftSpriteObject;

    void Start()
    {
        UpdateSprite();
    }

    void OnMouseDown()
    {
        goRight = !goRight;
        UpdateSprite();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySwitch();

        Debug.Log("Switch toggled: " + gameObject.name + " Go Right: " + goRight);

        // Notify InstructionsManager that this switch was tapped
        InstructionsManager instructionsManager = FindObjectOfType<InstructionsManager>();
        if (instructionsManager != null)
        {
            instructionsManager.OnSwitchTapped(gameObject);
        }
    }

    void UpdateSprite()
    {
        rightSpriteObject.SetActive(goRight);
        leftSpriteObject.SetActive(!goRight);
    }
}