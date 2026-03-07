using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public List<GameObject> colourPrefabs;  // 9 prefabs (one per colour)
    public Transform spawnPoint;
    public Transform firstJunction;

    public float startDelay = 2f;
    public float spawnInterval = 3f;
    public int maxBagsPerColour = 3;
    public int totalColours = 9;

    private int bagsSpawned = 0;
    private Dictionary<GameObject, int> spawnedCount = new Dictionary<GameObject, int>();
    private bool isSpawning = true;

    void Start()
    {
        foreach (GameObject prefab in colourPrefabs)
        {
            spawnedCount[prefab] = 0;
        }

        InvokeRepeating("SpawnBag", startDelay, spawnInterval);
    }

    void SpawnBag()
    {
        if (!isSpawning) return;

        if (bagsSpawned >= maxBagsPerColour * totalColours)
        {
            CancelInvoke("SpawnBag");
            Debug.Log("All 27 bags spawned!");
            return;
        }

        // Find available colours
        List<GameObject> availableColours = new List<GameObject>();

        foreach (GameObject prefab in colourPrefabs)
        {
            if (spawnedCount[prefab] < maxBagsPerColour)
            {
                availableColours.Add(prefab);
            }
        }

        if (availableColours.Count == 0) return;

        // Pick random colour
        int randomIndex = Random.Range(0, availableColours.Count);
        GameObject selectedPrefab = availableColours[randomIndex];

        // Create bag at spawn point
        GameObject newBag = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);

        // Set first target
        BagMovement bagMove = newBag.GetComponent<BagMovement>();
        if (bagMove != null)
        {
            bagMove.currentTarget = firstJunction;
        }

        // Update counts
        spawnedCount[selectedPrefab]++;
        bagsSpawned++;
    }

    public void StopSpawning()
    {
        isSpawning = false;
        CancelInvoke("SpawnBag");
    }
}