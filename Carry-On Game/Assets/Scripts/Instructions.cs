using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class InstructionsManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject instructionsPanel;
    public GameObject npcPanel;
    public TextMeshProUGUI npcMessageText;
    public Button npcButton;
    public Button skipButton;

    [Header("Tutorial Bag")]
    public GameObject tutorialBagPrefab;
    public Transform spawnPoint;
    public Transform firstJunction;

    [Header("Switches to Glow")]
    public List<GameObject> switchesToGlow;

    [Header("NPC Panel Slide Settings")]
    public float slideDuration = 0.3f;
    public Vector2 slideFromDirection = new Vector2(-1, 0);

    [Header("Messages")]
    public string welcomeMessage = "Hello {0}! Welcome to Carry-On!";
    public string instructionMessage = "Tap the glowing switch to change the bag's direction.";
    public string successMessage = "You got it!";
    public string readyMessage = "Are you ready to play?\n\nBags come faster every 7 points!";
    public string gotItButtonText = "Got it!";
    public string readyButtonText = "Let's Play!";
    public string skipButtonText = "Skip";

    private SpawnManager spawnManager;
    private string playerName = "Player";
    private GameObject currentTutorialBag;
    private bool tutorialComplete = false;
    private bool isTutorialActive = true;
    private int currentSwitchIndex = 0;
    private bool switchesCompleted = false;
    private List<JunctionGlow> switchGlowScripts = new List<JunctionGlow>();
    private RectTransform npcRectTransform;
    private Vector2 npcStartPosition;
    private Vector2 npcTargetPosition;
    private Coroutine currentMessageCoroutine;

    void Start()
    {
        spawnManager = FindObjectOfType<SpawnManager>();

        if (PlayerNameManager.Instance != null)
        {
            playerName = PlayerNameManager.Instance.CurrentPlayerName;
        }

        // Check if tutorial was already skipped - COMMENT THIS OUT FOR TESTING
        // bool tutorialSkipped = PlayerPrefs.GetInt("TutorialSkipped", 0) == 1;

        // if (tutorialSkipped)
        // {
        //     StartRealGame();
        //     return;
        // }

        // Store NPC panel RectTransform for sliding
        if (npcPanel != null)
        {
            npcRectTransform = npcPanel.GetComponent<RectTransform>();
            npcTargetPosition = npcRectTransform.anchoredPosition;
            float screenWidth = Screen.width;
            float offset = slideFromDirection.x * screenWidth;
            npcStartPosition = npcTargetPosition + new Vector2(offset, 0);
        }

        // Setup skip button
        if (skipButton != null)
        {
            // Set button text
            TextMeshProUGUI skipButtonTextComponent = skipButton.GetComponentInChildren<TextMeshProUGUI>();
            if (skipButtonTextComponent != null)
            {
                skipButtonTextComponent.text = skipButtonText;
            }
            skipButton.onClick.AddListener(SkipTutorial);
        }

        // Store switch glow scripts
        foreach (GameObject switchObj in switchesToGlow)
        {
            if (switchObj != null)
            {
                JunctionGlow glow = switchObj.GetComponent<JunctionGlow>();
                if (glow != null)
                {
                    switchGlowScripts.Add(glow);
                }
            }
        }

        // Stop normal spawning
        if (spawnManager != null)
        {
            spawnManager.StopSpawning();
        }

        // Hide UI initially
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
        if (npcPanel != null)
            npcPanel.SetActive(false);

        // Start tutorial
        StartCoroutine(TutorialSequence());
    }

    void SkipTutorial()
    {
        Debug.Log("Skip Tutorial button pressed - starting game immediately");

        // Save that tutorial was skipped
        PlayerPrefs.SetInt("TutorialSkipped", 1);
        PlayerPrefs.Save();

        // Stop any ongoing coroutines
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }

        // Clean up tutorial
        if (currentTutorialBag != null)
            Destroy(currentTutorialBag);

        // Hide all panels
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
        if (npcPanel != null)
            npcPanel.SetActive(false);

        // Stop all glows
        foreach (JunctionGlow glow in switchGlowScripts)
        {
            if (glow != null) glow.StopGlow();
        }

        // Start the real game
        StartRealGame();
    }

    IEnumerator ShowNPCMessage(string message, string buttonText)
    {
        if (npcPanel != null)
        {
            currentMessageCoroutine = StartCoroutine(ShowMessageInternal(message, buttonText));
            yield return currentMessageCoroutine;
            currentMessageCoroutine = null;
        }

        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator ShowMessageInternal(string message, string buttonText)
    {
        // Set position to start off-screen
        npcRectTransform.anchoredPosition = npcStartPosition;
        npcPanel.SetActive(true);

        if (npcMessageText != null)
            npcMessageText.text = message;

        if (npcButton != null)
        {
            TextMeshProUGUI buttonTextComponent = npcButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonTextComponent != null)
                buttonTextComponent.text = buttonText;

            npcButton.onClick.RemoveAllListeners();
        }

        // Make sure skip button is visible
        if (skipButton != null)
            skipButton.gameObject.SetActive(true);

        // Slide in
        yield return StartCoroutine(SlideInPanel());

        // Wait for button click
        bool buttonClicked = false;
        UnityEngine.Events.UnityAction clickAction = null;

        clickAction = () => {
            buttonClicked = true;
            npcButton.onClick.RemoveListener(clickAction);
        };

        npcButton.onClick.AddListener(clickAction);

        while (!buttonClicked)
        {
            yield return null;
        }

        // Slide out
        yield return StartCoroutine(SlideOutPanel());

        npcPanel.SetActive(false);
    }

    IEnumerator SlideInPanel()
    {
        float elapsedTime = 0;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideDuration;
            t = Mathf.SmoothStep(0, 1, t);
            npcRectTransform.anchoredPosition = Vector2.Lerp(npcStartPosition, npcTargetPosition, t);
            yield return null;
        }

        npcRectTransform.anchoredPosition = npcTargetPosition;
    }

    IEnumerator SlideOutPanel()
    {
        float elapsedTime = 0;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slideDuration;
            t = Mathf.SmoothStep(0, 1, t);
            npcRectTransform.anchoredPosition = Vector2.Lerp(npcTargetPosition, npcStartPosition, t);
            yield return null;
        }

        npcRectTransform.anchoredPosition = npcStartPosition;
    }

    public void OnSwitchTapped(GameObject tappedSwitch)
    {
        if (!isTutorialActive || switchesCompleted) return;

        if (currentSwitchIndex < switchesToGlow.Count &&
            switchesToGlow[currentSwitchIndex] == tappedSwitch)
        {
            if (switchGlowScripts[currentSwitchIndex] != null)
            {
                switchGlowScripts[currentSwitchIndex].StopGlow();
            }

            currentSwitchIndex++;

            if (currentSwitchIndex < switchesToGlow.Count)
            {
                if (switchGlowScripts[currentSwitchIndex] != null)
                {
                    switchGlowScripts[currentSwitchIndex].StartGlow();
                }
            }
            else
            {
                switchesCompleted = true;
                foreach (JunctionGlow glow in switchGlowScripts)
                {
                    if (glow != null) glow.StopGlow();
                }
            }
        }
    }

    IEnumerator TutorialSequence()
    {
        // First message - Welcome
        yield return StartCoroutine(ShowNPCMessage(string.Format(welcomeMessage, playerName) + "\n\n" + instructionMessage, gotItButtonText));

        // Show instruction panel
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);

        // Spawn tutorial bag
        SpawnTutorialBag();
        yield return new WaitForSeconds(1f);

        // Start glowing switches
        if (switchGlowScripts.Count > 0)
        {
            currentSwitchIndex = 0;
            if (switchGlowScripts[0] != null)
            {
                switchGlowScripts[0].StartGlow();
            }

            // Wait for all switches to be tapped
            while (!switchesCompleted)
            {
                yield return null;
            }
        }

        // Wait for bag to reach carousel
        yield return StartCoroutine(WaitForBagToFinish());

        // Success message
        yield return StartCoroutine(ShowNPCMessage(successMessage, gotItButtonText));

        // Hide skip button for final message
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        // Ready message
        yield return StartCoroutine(ShowNPCMessage(readyMessage, readyButtonText));

        StartRealGame();
    }

    void SpawnTutorialBag()
    {
        if (tutorialBagPrefab != null && spawnPoint != null && firstJunction != null)
        {
            currentTutorialBag = Instantiate(tutorialBagPrefab, spawnPoint.position, Quaternion.identity);
            currentTutorialBag.tag = "Bag";

            BagMovement bagMove = currentTutorialBag.GetComponent<BagMovement>();
            if (bagMove != null)
            {
                bagMove.currentTarget = firstJunction;
                bagMove.speed = 1f;
            }
        }
        else
        {
            Debug.LogError("Missing tutorial bag references!");
            StartRealGame();
        }
    }

    IEnumerator WaitForBagToFinish()
    {
        bool bagReachedCarousel = false;

        while (!bagReachedCarousel && currentTutorialBag != null)
        {
            CarouselColour[] carousels = FindObjectsOfType<CarouselColour>();
            foreach (CarouselColour carousel in carousels)
            {
                if (currentTutorialBag != null)
                {
                    float distance = Vector2.Distance(currentTutorialBag.transform.position, carousel.transform.position);
                    if (distance < 0.5f)
                    {
                        BagColour bagColour = currentTutorialBag.GetComponent<BagColour>();

                        if (bagColour != null && carousel.expectedLuggageColour == bagColour.luggageColour)
                        {
                            GlowEffect glow = carousel.GetComponent<GlowEffect>();
                            if (glow != null) glow.PlayGlow();

                            if (AudioManager.Instance != null)
                                AudioManager.Instance.PlayCorrect();

                            Destroy(currentTutorialBag);
                            bagReachedCarousel = true;
                        }
                        else
                        {
                            Destroy(currentTutorialBag);
                            yield return new WaitForSeconds(0.5f);
                            SpawnTutorialBag();
                        }
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);
    }

    void StartRealGame()
    {
        tutorialComplete = true;
        isTutorialActive = false;

        foreach (JunctionGlow glow in switchGlowScripts)
        {
            if (glow != null) glow.StopGlow();
        }

        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
        if (npcPanel != null)
            npcPanel.SetActive(false);

        if (spawnManager != null)
        {
            spawnManager.EnableSpawning();
        }

        if (currentTutorialBag != null)
            Destroy(currentTutorialBag);

        Debug.Log("Real game started!");
    }

    public bool IsTutorialComplete()
    {
        return tutorialComplete;
    }

    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }
}