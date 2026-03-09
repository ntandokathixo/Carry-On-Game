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

    private float currentSpawnInterval;
    private bool isSpawning = true;
    private GameManager gameManager;
    private bool isSpawnQueueRunning = false;

    void Start()
    {
        currentSpawnInterval = baseSpawnInterval;
        gameManager = FindObjectOfType<GameManager>();

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
        // Pick random colour
        int randomIndex = Random.Range(0, colourPrefabs.Count);
        GameObject selectedPrefab = colourPrefabs[randomIndex];

        // Create bag at exact spawn point (no offset)
        GameObject newBag = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);

        // Tag it
        newBag.tag = "Bag";

        // Set first target
        BagMovement bagMove = newBag.GetComponent<BagMovement>();
        if (bagMove != null)
        {
            bagMove.currentTarget = firstJunction;
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