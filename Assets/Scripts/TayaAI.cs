using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class TayaAI : MonoBehaviour
{
    private NavMeshAgent agent;
    public GameManager manager;
    private float retargetTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 0.5f;
        agent.autoBraking = false;
    }

    void Update()
    {
        if (manager == null || !manager.gameRunning) return;

        retargetTimer += Time.deltaTime;
        if (retargetTimer >= 0.5f)
        {
            Transform nearest = FindNearestTarget();
            if (nearest != null)
                agent.SetDestination(nearest.position);
            retargetTimer = 0f;
        }
    }

    Transform FindNearestTarget()
    {
        Transform nearest = null;
        float shortest = Mathf.Infinity;

        List<GameObject> allTargets = new List<GameObject>();
        allTargets.Add(manager.player);
        allTargets.AddRange(manager.friends);

        foreach (var target in allTargets)
        {
            if (target == null || target == gameObject) continue;
            if (target == manager.currentTaya) continue; // skip current taya

            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist < shortest)
            {
                shortest = dist;
                nearest = target.transform;
            }
        }

        return nearest;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!manager.gameRunning) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Friend"))
        {
            manager.SwapTaya(collision.gameObject);
        }
    }
}
