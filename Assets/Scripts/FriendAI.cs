using UnityEngine;
using UnityEngine.AI;

public class FriendAI : MonoBehaviour
{
    private NavMeshAgent agent;
    public bool canMove = false;
    public float wanderRadius = 8f;
    public float wanderDelay = 2f;

    private float nextMoveTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!canMove) return;

        if (Time.time >= nextMoveTime)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius);
            agent.SetDestination(newPos);
            nextMoveTime = Time.time + wanderDelay;
        }
    }

    Vector3 RandomNavSphere(Vector3 origin, float dist)
    {
        Vector3 randomDirection = Random.insideUnitSphere * dist;
        randomDirection += origin;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, dist, -1);
        return navHit.position;
    }
}
