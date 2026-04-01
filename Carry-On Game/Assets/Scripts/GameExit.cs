using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameExit: MonoBehaviour
{
    public Button exitButton;
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitToMenu);
        }
        else
        {
            exitButton = GetComponent<Button>();
            if (exitButton != null)
                exitButton.onClick.AddListener(ExitToMenu);
        }
    }

    void ExitToMenu()
    {
        Debug.Log("Returning to main menu...");

        // Optional: Stop game sounds or perform cleanup
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();

        // Load main menu
        SceneManager.LoadScene(mainMenuSceneName);
    }
}