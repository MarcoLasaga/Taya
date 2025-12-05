using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Settings")]
    public GameObject npcPrefab;
    public Transform[] spawnPoints;
    public Transform[] npcWaypoints;

    [Header("Spawn Settings")]
    public int maxNPCCount = 5;
    public float spawnIntervalMin = 3f;
    public float spawnIntervalMax = 8f;

    private List<GameObject> spawnedNPCs = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnNPCsRandomly());
    }

    IEnumerator SpawnNPCsRandomly()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));

            // Don’t exceed max count
            spawnedNPCs.RemoveAll(item => item == null);
            if (spawnedNPCs.Count >= maxNPCCount) continue;

            SpawnNPC();
        }
    }

    void SpawnNPC()
    {
        if (npcPrefab == null || spawnPoints.Length == 0) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject npc = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);

        NPCPatrol patrol = npc.GetComponent<NPCPatrol>();
        if (patrol != null)
            patrol.waypoints = npcWaypoints;

        spawnedNPCs.Add(npc);
    }
}
