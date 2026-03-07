using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public List<GameObject> colourPrefabs;
    public Transform spawnPoint;
    public Transform firstJunction;

    public float startDelay = 2f;
    public float spawnInterval = 3f;
    public int maxBagsPerColour = 3;
    public int totalColours = 9;

    private int bagsSpawned = 0;
    private Dictionary<GameObject, int> spawnedCount = new Dictionary<GameObject, int>();

    void Start()
    {
        Debug.Log("SpawnManager Start() called");

        if (colourPrefabs == null || colourPrefabs.Count == 0)
        {
            Debug.LogError("NO COLOUR PREFABS ASSIGNED!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("SPAWN POINT NOT ASSIGNED!");
            return;
        }

        if (firstJunction == null)
        {
            Debug.LogError("FIRST JUNCTION NOT ASSIGNED!");
            return;
        }

        foreach (GameObject prefab in colourPrefabs)
        {
            spawnedCount[prefab] = 0;
            Debug.Log("Added prefab: " + prefab.name);
        }

        Debug.Log("Starting InvokeRepeating with delay: " + startDelay + ", interval: " + spawnInterval);
        InvokeRepeating("SpawnBag", startDelay, spawnInterval);
    }

    void SpawnBag()
    {
        Debug.Log("SpawnBag() called at time: " + Time.time);

        if (bagsSpawned >= maxBagsPerColour * totalColours)
        {
            Debug.Log("All bags spawned, cancelling");
            CancelInvoke("SpawnBag");
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

        if (availableColours.Count == 0)
        {
            Debug.Log("No available colours");
            return;
        }

        int randomIndex = Random.Range(0, availableColours.Count);
        GameObject selectedPrefab = availableColours[randomIndex];

        Debug.Log("Attempting to spawn: " + selectedPrefab.name);
        Debug.Log("Spawn point position: " + spawnPoint.position);

        // Create the bag
        GameObject newBag = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);

        if (newBag == null)
        {
            Debug.LogError("Failed to instantiate bag!");
            return;
        }

        Debug.Log("Bag instantiated successfully: " + newBag.name);

        // Set first target
        BagMovement bagMove = newBag.GetComponent<BagMovement>();
        if (bagMove != null)
        {
            bagMove.currentTarget = firstJunction;
            Debug.Log("Set bag's currentTarget to: " + firstJunction.name);
        }
        else
        {
            Debug.LogError("Bag has no BagMovement script!");
        }

        spawnedCount[selectedPrefab]++;
        bagsSpawned++;

        Debug.Log("Spawned bag " + bagsSpawned + "/27. Colour: " + selectedPrefab.name);
    }
}