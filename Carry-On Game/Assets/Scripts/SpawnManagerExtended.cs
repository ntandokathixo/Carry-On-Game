using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManagerExtended : SpawnManager
{
    [Header("Extended Settings")]
    public List<GameObject> solidBagPrefabs;
    public List<GameObject> polkaDotBagPrefabs;
    public float polkaDotChance = 0.3f;

    protected override void SpawnBag()
    {
        if (!isSpawning) return;

        // Choose solid or polkadot
        bool isPolkaDot = Random.value < polkaDotChance;
        List<GameObject> activePrefabs = isPolkaDot ? polkaDotBagPrefabs : solidBagPrefabs;

        // Filter by colour availability
        List<GameObject> availablePrefabs = new List<GameObject>();

        foreach (GameObject prefab in activePrefabs)
        {
            BagColourExtended bagColour = prefab.GetComponent<BagColourExtended>();
            if (bagColour != null)
            {
                if (activeBagCount.ContainsKey(bagColour.luggageColour) &&
                    activeBagCount[bagColour.luggageColour] < maxBagsPerColour)
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

        // Track bag count
        if (selectedBagColour != null && activeBagCount.ContainsKey(selectedBagColour.luggageColour))
        {
            activeBagCount[selectedBagColour.luggageColour]++;
        }

        // Add tracker
        LuggageTracker tracker = newBag.AddComponent<LuggageTracker>();
        tracker.colour = selectedBagColour.luggageColour;
        tracker.spawnManager = this;

        // Set first target
        BagMovement bagMove = newBag.GetComponent<BagMovement>();
        if (bagMove != null)
        {
            bagMove.currentTarget = firstJunction;
        }
    }
}