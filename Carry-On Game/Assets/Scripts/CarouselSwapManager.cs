using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarouselSwapManager : MonoBehaviour
{
    public static CarouselSwapManager Instance;

    [Header("Swap Settings")]
    public int firstSwapScore = 42;
    public int swapInterval = 30;
    public int numberOfCarouselsToSwap = 2;
    public float swapAnimationDuration = 0.8f;
    public float pauseDuration = 2f;

    [Header("Visual Effects")]
    public AudioClip swapSound;

    [Header("NPC Panel")]
    public GameObject npcAnnouncementPanel;
    public TextMeshProUGUI announcementText;

    private List<CarouselColour> carousels = new List<CarouselColour>();
    private List<CarouselColour> carouselsToSwap = new List<CarouselColour>();
    private GameManager gameManager;
    private SpawnManager spawnManager;
    private bool hasSwapped = false;
    private int lastSwapScore = 0;
    private bool isSwapping = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        spawnManager = FindObjectOfType<SpawnManager>();

        CarouselColour[] foundCarousels = FindObjectsOfType<CarouselColour>();
        carousels.AddRange(foundCarousels);

        if (npcAnnouncementPanel != null)
            npcAnnouncementPanel.SetActive(false);

        Debug.Log("Found " + carousels.Count + " carousels");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(PerformCarouselSwap());
        }
    }

    public void CheckForSwap(int currentScore)
    {
        if (isSwapping) return;
        if (carousels.Count < 2) return;

        if (!hasSwapped && currentScore >= firstSwapScore)
        {
            hasSwapped = true;
            lastSwapScore = currentScore;
            StartCoroutine(PerformCarouselSwap());
        }
        else if (hasSwapped && currentScore - lastSwapScore >= swapInterval)
        {
            lastSwapScore = currentScore;
            StartCoroutine(PerformCarouselSwap());
        }
    }

    IEnumerator PerformCarouselSwap()
    {
        isSwapping = true;

        if (spawnManager != null)
            spawnManager.StopSpawning();

        DestroyAllActiveBags();
        Time.timeScale = 0f;

        yield return StartCoroutine(ShowAnnouncement());

        if (AudioManager.Instance != null && swapSound != null)
        {
            AudioManager.Instance.PlaySound(swapSound);
        }

        // Select which carousels will swap
        SelectCarouselsToSwap();

        // ALL carousels bounce
        StartAllCarouselBounces();

        // Wait 0.5 seconds
        yield return new WaitForSecondsRealtime(0.5f);

        // ONLY the selected carousels pulse twice
        StartSelectedCarouselPulses();

        // Wait 0.3 seconds for first pulse
        yield return new WaitForSecondsRealtime(0.3f);

        // Store waypoint connections before swap
        Dictionary<Waypoint, Transform> originalWaypointTargets = new Dictionary<Waypoint, Transform>();
        Waypoint[] allWaypoints = FindObjectsOfType<Waypoint>();
        foreach (Waypoint waypoint in allWaypoints)
        {
            if (waypoint.nextPoint != null)
            {
                originalWaypointTargets[waypoint] = waypoint.nextPoint;
            }
        }

        // Swap positions
        yield return StartCoroutine(SwapCarouselPositions());

        // Update waypoints to point to the correct carousels at their new positions
        UpdateWaypointTargets(originalWaypointTargets);

        // Wait a moment for swap to complete
        yield return new WaitForSecondsRealtime(0.5f);

        if (npcAnnouncementPanel != null)
            npcAnnouncementPanel.SetActive(false);

        Time.timeScale = 1f;

        if (spawnManager != null)
            spawnManager.EnableSpawning();

        isSwapping = false;

        Debug.Log("Carousel swap completed!");
    }

    void UpdateWaypointTargets(Dictionary<Waypoint, Transform> originalTargets)
    {
        // Create a mapping of carousel names to their new positions
        Dictionary<string, CarouselColour> carouselMap = new Dictionary<string, CarouselColour>();
        foreach (CarouselColour carousel in carousels)
        {
            carouselMap[carousel.name] = carousel;
        }

        foreach (var entry in originalTargets)
        {
            Waypoint waypoint = entry.Key;
            Transform originalTarget = entry.Value;

            // Check if the original target was a carousel
            CarouselColour targetCarousel = originalTarget.GetComponent<CarouselColour>();
            if (targetCarousel != null)
            {
                // Find the carousel that now has the same name (it moved)
                if (carouselMap.ContainsKey(targetCarousel.name))
                {
                    CarouselColour currentCarousel = carouselMap[targetCarousel.name];
                    waypoint.nextPoint = currentCarousel.transform;
                    Debug.Log("Updated waypoint " + waypoint.name + " to point to " + currentCarousel.name);
                }
            }
        }
    }

    void SelectCarouselsToSwap()
    {
        carouselsToSwap.Clear();
        List<CarouselColour> availableCarousels = new List<CarouselColour>(carousels);

        int swapCount = Mathf.Min(numberOfCarouselsToSwap, availableCarousels.Count);

        if (swapCount % 2 != 0)
            swapCount--;

        for (int i = 0; i < swapCount; i++)
        {
            int randomIndex = Random.Range(0, availableCarousels.Count);
            carouselsToSwap.Add(availableCarousels[randomIndex]);
            availableCarousels.RemoveAt(randomIndex);
        }

        Debug.Log("Selected " + carouselsToSwap.Count + " carousels to swap");
    }

    void StartAllCarouselBounces()
    {
        foreach (CarouselColour carousel in carousels)
        {
            if (carousel != null)
            {
                carousel.StartBounce();
            }
        }
        Debug.Log("Started bounce on all carousels");
    }

    void StartSelectedCarouselPulses()
    {
        foreach (CarouselColour carousel in carouselsToSwap)
        {
            if (carousel != null)
            {
                carousel.StartPulse();
                Debug.Log("Started double pulse on " + carousel.name);
            }
        }
    }

    void DestroyAllActiveBags()
    {
        BagMovement[] bags = FindObjectsOfType<BagMovement>();
        foreach (BagMovement bag in bags)
        {
            if (bag != null)
                Destroy(bag.gameObject);
        }
        Debug.Log("Destroyed " + bags.Length + " active bags");
    }

    IEnumerator ShowAnnouncement()
    {
        if (npcAnnouncementPanel != null)
        {
            npcAnnouncementPanel.SetActive(true);
            if (announcementText != null)
            {
                announcementText.text = "CAROUSEL SWAP!\nLook out! Carousels are moving positions!";
            }

            RectTransform rect = npcAnnouncementPanel.GetComponent<RectTransform>();

            // Store the target position (where it should end up)
            Vector2 endPos = rect.anchoredPosition;

            // Calculate start position off-screen (left side)
            Vector2 startPos = new Vector2(-Screen.width, endPos.y);

            // Set to start position
            rect.anchoredPosition = startPos;

            // Slide in
            float slideInDuration = 0.3f;
            float elapsedTime = 0;

            while (elapsedTime < slideInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = elapsedTime / slideInDuration;
                t = Mathf.SmoothStep(0, 1, t);
                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            rect.anchoredPosition = endPos;

            // Wait for the pause duration
            yield return new WaitForSecondsRealtime(pauseDuration);

            // Slide out
            float slideOutDuration = 0.3f;
            elapsedTime = 0;

            while (elapsedTime < slideOutDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = elapsedTime / slideOutDuration;
                t = Mathf.SmoothStep(0, 1, t);
                rect.anchoredPosition = Vector2.Lerp(endPos, startPos, t);
                yield return null;
            }

            rect.anchoredPosition = startPos;
            npcAnnouncementPanel.SetActive(false);
        }
    }
    IEnumerator SwapCarouselPositions()
    {
        for (int i = 0; i < carouselsToSwap.Count; i += 2)
        {
            if (i + 1 < carouselsToSwap.Count)
            {
                yield return StartCoroutine(SwapTwoCarousels(carouselsToSwap[i], carouselsToSwap[i + 1]));
            }
        }
    }

    IEnumerator SwapTwoCarousels(CarouselColour carouselA, CarouselColour carouselB)
    {
        Vector3 posA = carouselA.transform.position;
        Vector3 posB = carouselB.transform.position;

        float elapsedTime = 0;
        Vector3 startPosA = posA;
        Vector3 startPosB = posB;

        while (elapsedTime < swapAnimationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / swapAnimationDuration;
            t = Mathf.SmoothStep(0, 1, t);

            carouselA.transform.position = Vector3.Lerp(startPosA, posB, t);
            carouselB.transform.position = Vector3.Lerp(startPosB, posA, t);

            yield return null;
        }

        carouselA.transform.position = posB;
        carouselB.transform.position = posA;

        Debug.Log("Swapped positions of " + carouselA.name + " and " + carouselB.name);
    }

    public void ResetCarousels()
    {
        foreach (CarouselColour carousel in carousels)
        {
            if (carousel != null)
            {
                carousel.ResetToOriginal();
            }
        }
        hasSwapped = false;
        lastSwapScore = 0;
        carouselsToSwap.Clear();
        Debug.Log("Carousels reset");
    }
}