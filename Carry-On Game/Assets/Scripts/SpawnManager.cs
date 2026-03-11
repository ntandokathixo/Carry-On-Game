using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public List<GameObject> colourPrefabs;
    public Transform spawnPoint;
    public Transform firstJunction;

    public float startDelay = 2f;
    public float baseSpawnInterval = 3f;
    public float minSpawnInterval = 1f;
    public int pointsPerDifficultyIncrease = 7;
    public float minDistanceFromSpawn = 1.5f;  // How far bag must be before next spawn
    private Dictionary<LuggageColour, int> activeBagCount = new Dictionary<LuggageColour, int>();
    private int maxBagsPerColour = 2; // Maximum 3 of each colour at once

    private float currentSpawnInterval;
    private bool isSpawning = true;
    private GameManager gameManager;
    private bool isSpawnQueueRunning = false;

    void Start()
    {
        currentSpawnInterval = baseSpawnInterval;
        gameManager = FindObjectOfType<GameManager>();

        foreach (GameObject prefab in colourPrefabs)
        {
            BagColour bagColour = prefab.GetComponent<BagColour>();
            if (bagColour != null && !activeBagCount.ContainsKey(bagColour.luggageColour))
            {
                activeBagCount[bagColour.luggageColour] = 0;
            }
        }

        // Start the spawn queue
        StartCoroutine(SpawnQueue());
    }

    IEnumerator SpawnQueue()
    {
        yield return new WaitForSeconds(startDelay);

        while (isSpawning)
        {
            // Wait until the area near spawn point is clear
            yield return StartCoroutine(WaitForSpawnPointClear());

            // Spawn a bag
            SpawnBag();

            // Wait for the spawn interval
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    IEnumerator WaitForSpawnPointClear()
    {
        bool isClear = false;

        while (!isClear)
        {
            // Check if any bags are too close to spawn point
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(spawnPoint.position, minDistanceFromSpawn);

            isClear = true; // Assume clear until we find a bag

            foreach (Collider2D col in nearbyColliders)
            {
                if (col.CompareTag("Bag"))
                {
                    isClear = false;
                    break;
                }
            }

            if (!isClear)
            {
                // Wait a bit before checking again
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    void SpawnBag()
    {
        if (!isSpawning) return;

      
        // Find available colours (those with count < maxBagsPerColour)
        List<GameObject> availablePrefabs = new List<GameObject>();

        foreach (GameObject prefab in colourPrefabs)
        {
            BagColour bagColour = prefab.GetComponent<BagColour>();
            if (bagColour != null)
            {
                // Check if we haven't reached the limit for this colour
                if (activeBagCount.ContainsKey(bagColour.luggageColour) &&
                    activeBagCount[bagColour.luggageColour] < maxBagsPerColour)
                {
                    availablePrefabs.Add(prefab);
                }
            }
        }

        // If no colours available, wait and try again
        if (availablePrefabs.Count == 0)
        {
            Debug.Log("All colours at max capacity - waiting for bags to be destroyed");
            return;
        }

        // Pick random colour from available ones
        int randomIndex = Random.Range(0, availablePrefabs.Count);
        GameObject selectedPrefab = availablePrefabs[randomIndex];
        BagColour selectedBagColour = selectedPrefab.GetComponent<BagColour>();

        // Create bag
        GameObject newBag = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);
        newBag.tag = "Bag";

        
        // Add a component to track when this bag is destroyed
        LuggageTracker tracker = newBag.AddComponent<LuggageTracker>();
        tracker.colour = selectedBagColour.luggageColour;
        tracker.spawnManager = this;

       
        // Increment count for this colour
        if (selectedBagColour != null && activeBagCount.ContainsKey(selectedBagColour.luggageColour))
        {
            activeBagCount[selectedBagColour.luggageColour]++;
            Debug.Log("Spawned " + selectedBagColour.luggageColour + " bag. Total active: " +
                      activeBagCount[selectedBagColour.luggageColour] + "/3");
        }

        // Set first target
        BagMovement bagMove = newBag.GetComponent<BagMovement>();
        if (bagMove != null)
        {
            bagMove.currentTarget = firstJunction;
        }
    }

    public void OnBagDestroyed(LuggageColour colour)
    {
        if (activeBagCount.ContainsKey(colour))
        {
            activeBagCount[colour] = Mathf.Max(0, activeBagCount[colour] - 1);
            Debug.Log(colour + " bag destroyed. Remaining: " + activeBagCount[colour] + "/3");
        }
    }


    public void OnScoreIncreased(int newScore)
    {
        if (!isSpawning) return;

        int difficultySteps = newScore / pointsPerDifficultyIncrease;
        float newInterval = baseSpawnInterval - (difficultySteps * 0.3f);
        currentSpawnInterval = Mathf.Max(minSpawnInterval, newInterval);

        // No need to restart coroutine - it will use new interval on next loop
        Debug.Log("Difficulty increased! New spawn interval: " + currentSpawnInterval);
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
}