using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManagerExtended : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<GameObject> solidBagPrefabs;
    public List<GameObject> polkaDotBagPrefabs;
    public float polkaDotChance = 0.3f;
    public Transform spawnPoint;
    public Transform firstJunction;
    public float startDelay = 3f;
    public float baseSpawnInterval = 5.5f;
    public float minSpawnInterval = 3.2f;
    public float minDistanceFromSpawn = 1.5f;
    public int maxBagsPerColour = 3;
    public int pointsPerDifficultyIncrease = 7;

    private Dictionary<LuggageColour, int> activeBagCount = new Dictionary<LuggageColour, int>();
    private float currentSpawnInterval;
    private bool isSpawning = false;
    private GameManager gameManager;
    private float lastSpawnTime = 0f;
    private int lastDifficultyScore = 0;
    private int totalBagsSpawned = 0;  // Only declared once

    void Start()
    {
        currentSpawnInterval = baseSpawnInterval;
        gameManager = FindObjectOfType<GameManager>();

        // Initialize bag counts for solid bags
        foreach (GameObject prefab in solidBagPrefabs)
        {
            BagColourExtended bagColour = prefab.GetComponent<BagColourExtended>();
            if (bagColour != null && !activeBagCount.ContainsKey(bagColour.luggageColour))
            {
                activeBagCount[bagColour.luggageColour] = 0;
            }
        }

        // Initialize bag counts for polka dot bags
        foreach (GameObject prefab in polkaDotBagPrefabs)
        {
            BagColourExtended bagColour = prefab.GetComponent<BagColourExtended>();
            if (bagColour != null && !activeBagCount.ContainsKey(bagColour.luggageColour))
            {
                activeBagCount[bagColour.luggageColour] = 0;
            }
        }

        Debug.Log($"Spawn interval starts at {currentSpawnInterval}s and will decrease to {minSpawnInterval}s");
    }

    public void StartSpawning()
    {
        Debug.Log($"StartSpawning called. Initial interval: {baseSpawnInterval}s");

        if (solidBagPrefabs.Count == 0 && polkaDotBagPrefabs.Count == 0)
        {
            Debug.LogError("No bag prefabs assigned!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("spawnPoint is NULL!");
            return;
        }

        if (firstJunction == null)
        {
            Debug.LogError("firstJunction is NULL!");
            return;
        }

        if (isSpawning)
        {
            Debug.Log("Already spawning!");
            return;
        }

        isSpawning = true;
        StartCoroutine(SpawnQueue());
    }

    IEnumerator SpawnQueue()
    {
        Debug.Log($"SpawnQueue started. First bag in {startDelay}s, interval: {currentSpawnInterval}s");
        yield return new WaitForSeconds(startDelay);

        while (isSpawning)
        {
            yield return StartCoroutine(WaitForSpawnPointClear());
            SpawnBag();
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    IEnumerator WaitForSpawnPointClear()
    {
        bool isClear = false;

        while (!isClear)
        {
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(spawnPoint.position, minDistanceFromSpawn);

            isClear = true;
            foreach (Collider2D col in nearbyColliders)
            {
                if (col != null && col.CompareTag("Bag"))
                {
                    isClear = false;
                    break;
                }
            }

            if (!isClear)
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    void SpawnBag()
    {
        if (!isSpawning) return;

        bool isPolkaDot = Random.value < polkaDotChance;
        List<GameObject> activePrefabs = isPolkaDot ? polkaDotBagPrefabs : solidBagPrefabs;

        if (activePrefabs.Count == 0) return;

        List<GameObject> availablePrefabs = new List<GameObject>();

        foreach (GameObject prefab in activePrefabs)
        {
            BagColourExtended bagColour = prefab.GetComponent<BagColourExtended>();
            if (bagColour != null)
            {
                int currentCount = activeBagCount.ContainsKey(bagColour.luggageColour) ? activeBagCount[bagColour.luggageColour] : 0;
                if (currentCount < maxBagsPerColour)
                {
                    availablePrefabs.Add(prefab);
                }
            }
        }

        if (availablePrefabs.Count == 0) return;

        int randomIndex = Random.Range(0, availablePrefabs.Count);
        GameObject selectedPrefab = availablePrefabs[randomIndex];
        BagColourExtended selectedBagColour = selectedPrefab.GetComponent<BagColourExtended>();

        GameObject newBag = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);
        newBag.tag = "Bag";

        if (selectedBagColour != null && activeBagCount.ContainsKey(selectedBagColour.luggageColour))
        {
            activeBagCount[selectedBagColour.luggageColour]++;
        }

        LuggageTracker tracker = newBag.AddComponent<LuggageTracker>();
        tracker.colour = selectedBagColour.luggageColour;
        tracker.spawnManager = this;

        BagMovement bagMove = newBag.GetComponent<BagMovement>();
        if (bagMove != null)
        {
            bagMove.currentTarget = firstJunction;
        }

        lastSpawnTime = Time.time;
        totalBagsSpawned++;
        Debug.Log($"Total bags spawned: {totalBagsSpawned}");
    }

    public void OnBagDestroyed(LuggageColour colour)
    {
        if (activeBagCount.ContainsKey(colour))
        {
            activeBagCount[colour] = Mathf.Max(0, activeBagCount[colour] - 1);
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
        Debug.Log("Spawning stopped");
    }

    public float GetCurrentSpawnInterval()
    {
        return currentSpawnInterval;
    }

    public void OnScoreIncreased(int newScore)
    {
        if (!isSpawning) return;

        int currentStep = newScore / pointsPerDifficultyIncrease;
        int lastStep = lastDifficultyScore / pointsPerDifficultyIncrease;

        if (currentStep > lastStep)
        {
            float newInterval = baseSpawnInterval - (currentStep * 0.12f);
            currentSpawnInterval = Mathf.Max(minSpawnInterval, newInterval);
            lastDifficultyScore = newScore;

            Debug.Log($"Score {newScore}: Spawn interval now {currentSpawnInterval:F2}s");
        }
    }

    public int GetTotalBagsSpawned()
    {
        return totalBagsSpawned;
    }
}