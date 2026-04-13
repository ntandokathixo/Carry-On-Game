using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public List<GameObject> colourPrefabs;
    public Transform spawnPoint;
    public Transform firstJunction;

    public float startDelay = 2f;
    public float baseSpawnInterval = 4f;
    public float minSpawnInterval = 1f;
    public int pointsPerDifficultyIncrease = 7;
    public float minDistanceFromSpawn = 1.5f;
    private Dictionary<LuggageColour, int> activeBagCount = new Dictionary<LuggageColour, int>();
    private int maxBagsPerColour = 2;

    private float currentSpawnInterval;
    private bool isSpawning = false;
    private GameManager gameManager;

    [Header("Busyness Meter")]
    public int totalBagsSpawned = 0;
    public int bagsForMaxBusyness = 35;

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
    }

    public int GetTotalBagsSpawned()
    {
        return totalBagsSpawned;
    }

    public void EnableSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnQueue());
            Debug.Log("Spawning enabled!");
        }
    }

    IEnumerator SpawnQueue()
    {
        yield return new WaitForSeconds(startDelay);

        while (isSpawning)
        {
            yield return StartCoroutine(WaitForSpawnPointClear());
            SpawnBag();
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }
    public float GetCurrentSpawnInterval()
    {
        return currentSpawnInterval;
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
                if (col.CompareTag("Bag"))
                {
                    isClear = false;
                    break;
                }
            }

            if (!isClear)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    void SpawnBag()
    {
        if (!isSpawning) return;

        List<GameObject> availablePrefabs = new List<GameObject>();

        foreach (GameObject prefab in colourPrefabs)
        {
            BagColour bagColour = prefab.GetComponent<BagColour>();
            if (bagColour != null)
            {
                if (activeBagCount.ContainsKey(bagColour.luggageColour) &&
                    activeBagCount[bagColour.luggageColour] < maxBagsPerColour)
                {
                    availablePrefabs.Add(prefab);
                }
            }
        }

        if (availablePrefabs.Count == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, availablePrefabs.Count);
        GameObject selectedPrefab = availablePrefabs[randomIndex];
        BagColour selectedBagColour = selectedPrefab.GetComponent<BagColour>();

        GameObject newBag = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);
        newBag.tag = "Bag";

        LuggageTracker tracker = newBag.AddComponent<LuggageTracker>();
        tracker.colour = selectedBagColour.luggageColour;
        tracker.spawnManager = this;

        if (selectedBagColour != null && activeBagCount.ContainsKey(selectedBagColour.luggageColour))
        {
            activeBagCount[selectedBagColour.luggageColour]++;
        }

        BagMovement bagMove = newBag.GetComponent<BagMovement>();
        if (bagMove != null)
        {
            bagMove.currentTarget = firstJunction;
        }

        totalBagsSpawned++;

        BusynessMeter meter = FindObjectOfType<BusynessMeter>();
        if (meter != null)
        {
            meter.OnBagSpawned(totalBagsSpawned);
        }
    }

    public void OnBagDestroyed(LuggageColour colour)
    {
        if (activeBagCount.ContainsKey(colour))
        {
            activeBagCount[colour] = Mathf.Max(0, activeBagCount[colour] - 1);
        }
    }

    public void OnScoreIncreased(int newScore)
    {
        if (!isSpawning) return;

        int difficultySteps = newScore / pointsPerDifficultyIncrease;
        float newInterval = baseSpawnInterval - (difficultySteps * 0.3f);
        currentSpawnInterval = Mathf.Max(minSpawnInterval, newInterval);
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
}