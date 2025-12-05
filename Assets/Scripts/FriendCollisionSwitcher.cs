using UnityEngine;

// Attach this to friend NPC GameObjects. When two friends collide,
// this component will ask the GameManager to switch the Taya.
//
// Behavior:
// - If `useTag` is true, collisions with GameObjects tagged "Friend"
//   will trigger a potential swap.
// - If `requireInFriendsList` is true, both objects must be present in
//   `GameManager.friends` for the swap to occur.
// - To avoid double-switching (both colliders firing), only the object
//   with the lower instance ID will perform the swap action.
public class FriendCollisionSwitcher : MonoBehaviour
{
    [Tooltip("If true, collision partner must have the 'Friend' tag to consider switching.")]
    public bool useTag = true;

    [Tooltip("If true, both colliders must be present in GameManager.friends list to allow switching.")]
    public bool requireInFriendsList = false;
    [Header("Debug Tag Speeds (friend-to-friend)")]
    [Tooltip("When true, apply temporary speed changes to the tagger (fast) and tagged (slow)")]
    public bool debugTagSpeeds = true;
    public float debugTayaSpeedOnTag = 15f;
    public float debugEscapeeSpeedOnTag = 0.5f;
    public float debugTagDuration = 2f;
    [Header("Proximity Fallback")]
    [Tooltip("If true, periodically check nearby friends by distance and attempt swaps when colliders don't generate OnCollision events.")]
    public bool enableProximitySwap = true;
    public float proximityRadius = 0.5f;
    public float proximityCheckInterval = 0.2f;
    private float lastProximityCheck = 0f;

    void OnCollisionEnter(Collision col)
    {
        GameObject other = col.gameObject;

        // Quick tag check
        if (useTag && !other.CompareTag("Friend"))
            return;

        var gm = FindObjectOfType<GameManager>();
        if (gm == null) return;

        // If required, ensure both objects are in the GameManager.friends list
        if (requireInFriendsList)
        {
            if (gm.friends == null) return;
            if (!gm.friends.Contains(gameObject) || !gm.friends.Contains(other))
                return;
        }

        // Debug: log the collision and candidate
        Debug.Log($"[FriendCollisionSwitcher] {gameObject.name} collided with {other.name}. useTag={useTag}, requireInFriendsList={requireInFriendsList}");

        // Prevent both colliders from triggering a swap at once;
        // only the object with the lower InstanceID triggers the action.
        if (this.GetInstanceID() >= other.GetInstanceID())
            return;

        // Ask GameManager to make the collided object the new Taya.
        Debug.Log($"[FriendCollisionSwitcher] Requesting SwapTaya -> {other.name}");
        bool swapped = gm.TrySwapTaya(other);
        if (swapped)
        {
            Debug.Log($"[FriendCollisionSwitcher] Swap succeeded -> {other.name}");
            // If both objects have NPCStateMachine and debug speeds enabled,
            // apply a temporary speed boost to the tagger (this) and slow
            // down the tagged friend for visibility during debugging.
            if (debugTagSpeeds)
            {
                var mySM = GetComponent<NPCStateMachine>();
                var otherSM = other.GetComponent<NPCStateMachine>();
                if (mySM != null && otherSM != null)
                {
                    // Make sure the one initiating the swap (this) is treated as Taya
                    mySM.ApplyTemporarySpeed(debugTayaSpeedOnTag, debugTagDuration);
                    otherSM.ApplyTemporarySpeed(debugEscapeeSpeedOnTag, debugTagDuration);
                    Debug.Log($"[FriendCollisionSwitcher] Applied debug speeds: {gameObject.name} -> {debugTayaSpeedOnTag}, {other.name} -> {debugEscapeeSpeedOnTag} for {debugTagDuration}s");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[FriendCollisionSwitcher] Swap failed for {other.name}; currentTaya={gm.currentTaya?.name}");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Support trigger-based collisions as well.
        if (useTag && !other.CompareTag("Friend")) return;

        var go = other.gameObject;
        var gm = FindObjectOfType<GameManager>();
        if (gm == null) return;

        Debug.Log($"[FriendCollisionSwitcher] Trigger detected: {gameObject.name} hit {go.name}");
        bool swapped = gm.TrySwapTaya(go);
        if (swapped)
        {
            if (debugTagSpeeds)
            {
                var mySM = GetComponent<NPCStateMachine>();
                var otherSM = go.GetComponent<NPCStateMachine>();
                if (mySM != null && otherSM != null)
                {
                    mySM.ApplyTemporarySpeed(debugTayaSpeedOnTag, debugTagDuration);
                    otherSM.ApplyTemporarySpeed(debugEscapeeSpeedOnTag, debugTagDuration);
                }
            }
        }
        else
        {
            Debug.LogWarning($"[FriendCollisionSwitcher] Trigger swap failed for {go.name}; currentTaya={gm.currentTaya?.name}");
        }
    }

    void Update()
    {
        if (!enableProximitySwap) return;

        if (Time.time - lastProximityCheck < proximityCheckInterval) return;
        lastProximityCheck = Time.time;

        // Overlap sphere to find colliders close enough to consider a contact
        Collider[] hits = Physics.OverlapSphere(transform.position, proximityRadius);
        if (hits == null || hits.Length == 0) return;

        var gm = FindObjectOfType<GameManager>();
        if (gm == null) return;

        foreach (var hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;
            if (useTag && !hit.CompareTag("Friend")) continue;

            // If requireInFriendsList is set, ensure both are in list
            if (requireInFriendsList)
            {
                if (gm.friends == null) continue;
                if (!gm.friends.Contains(this.gameObject) || !gm.friends.Contains(hit.gameObject)) continue;
            }

            // attempt swap
            Debug.Log($"[FriendCollisionSwitcher] Proximity candidate: {gameObject.name} -> {hit.gameObject.name}");
            bool swapped = gm.TrySwapTaya(hit.gameObject);
            if (swapped)
            {
                Debug.Log($"[FriendCollisionSwitcher] Proximity swap succeeded: {hit.gameObject.name}");
                if (debugTagSpeeds)
                {
                    var mySM = GetComponent<NPCStateMachine>();
                    var otherSM = hit.gameObject.GetComponent<NPCStateMachine>();
                    if (mySM != null && otherSM != null)
                    {
                        mySM.ApplyTemporarySpeed(debugTayaSpeedOnTag, debugTagDuration);
                        otherSM.ApplyTemporarySpeed(debugEscapeeSpeedOnTag, debugTagDuration);
                    }
                }
                break; // stop after a successful proximity swap
            }
        }
    }
}
