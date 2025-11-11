using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TayaAI : MonoBehaviour
{
    public Transform player; // assign the player capsule here in Inspector
    public float chaseRange = 30f; // how far taya can detect the player
    public float catchDistance = 1.5f; // distance where taya catches player

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        // If player is within chase range, start chasing
        if (distance <= chaseRange)
        {
            agent.SetDestination(player.position);

            // Optional: Face the player while moving
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction.magnitude > 0.1f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }
        else
        {
            // Stop moving when out of range
            agent.ResetPath();
        }

        // If caught the player
        if (distance <= catchDistance)
        {
            Debug.Log("Taya caught the player!");
            agent.ResetPath();
        }
    }
}
