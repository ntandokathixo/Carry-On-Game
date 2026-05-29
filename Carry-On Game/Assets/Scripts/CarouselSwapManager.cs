using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarouselSwapManager : MonoBehaviour
{
    public static CarouselSwapManager Instance;

    [Header("Swap Settings")]
    public int firstSwapScore = 28;
    public int swapInterval = 21;
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
        {
            // Position panel off-screen initially (keep it active)
            RectTransform rect = npcAnnouncementPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2 currentPos = rect.anchoredPosition;
                rect.anchoredPosition = new Vector2(-Screen.width, currentPos.y);
            }
            npcAnnouncementPanel.SetActive(true);
        }

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

        SelectCarouselsToSwap();
        StartAllCarouselBounces();

        yield return new WaitForSecondsRealtime(0.5f);

        StartSelectedCarouselPulses();

        yield return new WaitForSecondsRealtime(0.3f);

        Dictionary<Waypoint, Transform> originalWaypointTargets = new Dictionary<Waypoint, Transform>();
        Waypoint[] allWaypoints = FindObjectsOfType<Waypoint>();
        foreach (Waypoint waypoint in allWaypoints)
        {
            if (waypoint.nextPoint != null)
            {
                originalWaypointTargets[waypoint] = waypoint.nextPoint;
            }
        }

        yield return StartCoroutine(SwapCarouselPositions());

        UpdateWaypointTargets(originalWaypointTargets);

        yield return new WaitForSecondsRealtime(0.5f);

        if (gameManager != null)
        {
            gameManager.OnCarouselSwapOccurred();
        }

        Time.timeScale = 1f;

        if (spawnManager != null)
            spawnManager.EnableSpawning();

        isSwapping = false;

        Debug.Log("Carousel swap completed!");
    }

    IEnumerator ShowAnnouncement()
    {
        if (npcAnnouncementPanel != null && announcementText != null)
        {
            // Update text
            announcementText.text = "CAROUSEL SWAP!\nLook out! Carousels are moving positions!";

            RectTransform rect = npcAnnouncementPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Store target position (where it should end up)
                Vector2 targetPos = new Vector2(0, rect.anchoredPosition.y);

                // Set start position (off-screen left)
                Vector2 startPos = new Vector2(-Screen.width, targetPos.y);
                rect.anchoredPosition = startPos;

                // Slide in
                float slideInDuration = 0.3f;
                float elapsedTime = 0;

                while (elapsedTime < slideInDuration)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    float t = elapsedTime / slideInDuration;
                    t = Mathf.SmoothStep(0, 1, t);
                    rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                    yield return null;
                }

                rect.anchoredPosition = targetPos;
            }

            // Wait
            yield return new WaitForSecondsRealtime(pauseDuration);

            // Slide out
            if (rect != null)
            {
                Vector2 currentPos = rect.anchoredPosition;
                Vector2 offScreenPos = new Vector2(-Screen.width, currentPos.y);

                float slideOutDuration = 0.3f;
                float elapsedTime = 0;

                while (elapsedTime < slideOutDuration)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    float t = elapsedTime / slideOutDuration;
                    t = Mathf.SmoothStep(0, 1, t);
                    rect.anchoredPosition = Vector2.Lerp(currentPos, offScreenPos, t);
                    yield return null;
                }

                rect.anchoredPosition = offScreenPos;
            }
        }
    }

    void UpdateWaypointTargets(Dictionary<Waypoint, Transform> originalTargets)
    {
        Dictionary<string, CarouselColour> carouselMap = new Dictionary<string, CarouselColour>();
        foreach (CarouselColour carousel in carousels)
        {
            carouselMap[carousel.name] = carousel;
        }

        foreach (var entry in originalTargets)
        {
            Waypoint waypoint = entry.Key;
            Transform originalTarget = entry.Value;

            CarouselColour targetCarousel = originalTarget.GetComponent<CarouselColour>();
            if (targetCarousel != null)
            {
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