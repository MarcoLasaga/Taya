using UnityEngine;
using System.Collections;

public class NPCPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float waitTime = 1f;
    public float obstacleDetectDistance = 1f;

    private int currentWaypoint = 0;
    private bool isWaiting = false;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0 || isWaiting)
            return;

        // Detect obstacles ahead
        if (Physics.Raycast(transform.position, transform.forward, obstacleDetectDistance))
        {
            TurnAround();
            return;
        }

        MoveToNextWaypoint();
    }

    void MoveToNextWaypoint()
    {
        Transform target = waypoints[currentWaypoint];
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // Rotate smoothly toward the next waypoint
        Vector3 direction = (target.position - transform.position).normalized;
        if (direction.magnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // Reached waypoint
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            if (currentWaypoint >= waypoints.Length - 1)
            {
                // Vanish after final waypoint
                Destroy(gameObject, 0.5f);
                return;
            }
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        currentWaypoint++;
        isWaiting = false;
    }

    void TurnAround()
    {
        // Rotate 180 degrees to move away from obstacle
        transform.Rotate(0f, 180f, 0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * obstacleDetectDistance);
    }
}
