using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

public class Level3Controller : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The capsule (or object) that will be transformed into the hallucination prefab")]
    public GameObject targetCapsule;

    [Tooltip("Prefab to spawn in place of the capsule (hallucination)")]
    public GameObject hallucinationPrefab;

    [Header("Behavior")]
    [Tooltip("If true the original capsule GameObject will be destroyed after transform; otherwise it will be deactivated")]
    public bool destroyOriginal = true;

    [Tooltip("Time (seconds) after transform when the hallucination will get faster")]
    public float speedupTime = 90f; // 1:30

    [Tooltip("Multiplier applied to movement-related speeds when the speedup triggers")]
    public float speedMultiplier = 2f;

    [Tooltip("If true the transform will occur automatically on Start()")]
    public bool autoActivate = false;

    [Tooltip("If true, transform occurs when player enters this object's trigger collider")]
    public bool triggerOnPlayerEnter = false;

    [Header("Audio")]
    [Tooltip("Optional audio source to play transform SFX; will be created if null")]
    public AudioSource sfxSource;
    [Tooltip("Sound to play when the capsule transforms into the hallucination")]
    public AudioClip transformClip;

    private GameObject hallucinationInstance;
    private bool transformed = false;
    private float elapsedSinceTransform = 0f;
    private bool speedupApplied = false;
    [Header("Timer Link")]
    [Tooltip("If set, Level3Controller will watch the GameManager timer and trigger the transform when the remaining time reaches this value (seconds).")]
    public float transformAtRemainingTime = 90f;

    private GameManager gm;
    private float lastObservedRemaining = float.PositiveInfinity;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        if (gm == null)
            Debug.LogWarning("[Level3Controller] No GameManager found in scene. Timer-based transform won't work.");
        else
            Debug.Log("[Level3Controller] Found GameManager for timer linkage.");
        if (autoActivate)
            TransformCapsule();
    }

    void Update()
    {
        // If a GameManager is present and we're not yet transformed, check timer.
        // Use crossing-detection (lastObservedRemaining) so pauses/unpauses don't prevent the event.
        if (!transformed && gm != null)
        {
            float current = gm.RemainingTime;
            Debug.Log($"[Level3Controller] Debug: RemainingTime={current:F2}, lastObserved={lastObservedRemaining:F2}, gameRunning={gm.gameRunning}, transformed={transformed}");

            // Trigger when the timer crosses from above -> below the threshold.
            if (current <= transformAtRemainingTime && lastObservedRemaining > transformAtRemainingTime)
            {
                Debug.Log($"[Level3Controller] Timer crossed threshold ({current:F1}s -> {transformAtRemainingTime}s). Triggering transform.");
                DoTransformAndForceTaya();
            }

            lastObservedRemaining = current;
        }

        // Manual debug trigger: press K to force the transform (helps testing pause/unpause)
        if (!transformed && Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("[Level3Controller] Manual K press: forcing transform.");
            DoTransformAndForceTaya();
        }

        // After transform, keep the hallucination as Taya even after gameRunning ends
        // (so it continues to move and behave, instead of being reset to wander)
        if (transformed && hallucinationInstance != null && gm != null)
        {
            // If the game ended but the hallucination is no longer set as currentTaya,
            // re-apply Taya state to keep it moving
            if (gm.currentTaya != hallucinationInstance && hallucinationInstance.activeInHierarchy)
            {
                var sm = hallucinationInstance.GetComponent<NPCStateMachine>();
                if (sm != null && sm.currentState != sm.tayaState)
                {
                    Debug.Log("[Level3Controller] Re-applying TayaState to hallucination to keep it moving.");
                    gm.currentTaya = hallucinationInstance;
                    sm.SwitchState(sm.tayaState);
                }
            }
        }

        if (!transformed) return;

        elapsedSinceTransform += Time.deltaTime;
        if (!speedupApplied && elapsedSinceTransform >= speedupTime)
        {
            ApplySpeedupToHallucination();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerOnPlayerEnter) return;
        if (!other.CompareTag("Player")) return;
        TransformCapsule();
    }

    // Public: trigger the transform manually
    public void TransformCapsule()
    {
        if (transformed) return;
        if (targetCapsule == null || hallucinationPrefab == null)
        {
            Debug.LogWarning("[Level3Controller] Missing targetCapsule or hallucinationPrefab");
            return;
        }

        Vector3 pos = targetCapsule.transform.position;
        Quaternion rot = targetCapsule.transform.rotation;
        Vector3 scale = targetCapsule.transform.localScale;

        hallucinationInstance = Instantiate(hallucinationPrefab, pos, rot);
        hallucinationInstance.transform.localScale = scale;

        // Try to copy simple runtime state (velocity) if both have rigidbodies
        var origRb = targetCapsule.GetComponent<Rigidbody>();
        var instRb = hallucinationInstance.GetComponent<Rigidbody>();
        if (origRb != null && instRb != null)
        {
            instRb.velocity = origRb.velocity;
            instRb.angularVelocity = origRb.angularVelocity;
        }

        if (destroyOriginal)
            Destroy(targetCapsule);
        else
            targetCapsule.SetActive(false);

        // Play optional transform sound for depth
        if (transformClip != null)
        {
            AudioSource src = sfxSource;
            if (src == null)
            {
                src = GetComponent<AudioSource>();
                if (src == null)
                    src = gameObject.AddComponent<AudioSource>();
                sfxSource = src;
            }
            src.PlayOneShot(transformClip);
        }

        transformed = true;
        elapsedSinceTransform = 0f;
        speedupApplied = false;

        Debug.Log("[Level3Controller] Capsule transformed into hallucination.");
    }

    private void DoTransformAndForceTaya()
    {
        TransformCapsule();

        if (hallucinationInstance != null && gm != null)
        {
            // Ensure hallucination is considered a friend by the GameManager
            try
            {
                if (gm.friends != null && !gm.friends.Contains(hallucinationInstance))
                {
                    gm.friends.Add(hallucinationInstance);
                    Debug.Log("[Level3Controller] Added hallucination to GameManager.friends list.");
                }
            }
            catch { }

            // Tag as friend if possible
            try { hallucinationInstance.tag = "Hallucination"; } catch { }            // Ensure NavMeshAgent exists and NPCStateMachine exists and are wired
            var agent = hallucinationInstance.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = hallucinationInstance.AddComponent<NavMeshAgent>();
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                agent.stoppingDistance = 0f;
                agent.radius = 0.1f;
                agent.autoBraking = false;
                Debug.Log("[Level3Controller] Added NavMeshAgent to hallucination instance.");
            }

            var sm = hallucinationInstance.GetComponent<NPCStateMachine>();
            if (sm == null)
            {
                sm = hallucinationInstance.AddComponent<NPCStateMachine>();
                Debug.Log("[Level3Controller] Added NPCStateMachine to hallucination instance.");
            }

            if (gm != null) sm.GM = gm;
            if (sm.agent == null && agent != null) sm.agent = agent;
            try { agent.speed = sm != null ? sm.wanderSpeed : agent.speed; } catch { }

            // Force the swap and speedup
            gm.SwapTaya(hallucinationInstance);
            Debug.Log("[Level3Controller] Forced SwapTaya to hallucination instance.");

            if (!speedupApplied)
                ApplySpeedupToHallucination();
        }
    }

    private void ApplySpeedupToHallucination()
    {
        if (hallucinationInstance == null)
        {
            speedupApplied = true; // nothing to do
            return;
        }

        // 1) If there's a NavMeshAgent, increase its speed
        var agents = hallucinationInstance.GetComponentsInChildren<NavMeshAgent>();
        foreach (var a in agents)
        {
            a.speed *= speedMultiplier;
            a.acceleration *= speedMultiplier;
        }

        // 2) If it has rigidbodies that are being driven by scripts, try to find 'moveSpeed'/'walkSpeed' fields
        var components = hallucinationInstance.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var comp in components)
        {
            if (comp == null) continue;
            var t = comp.GetType();
            // common speed field names used in this project
            string[] names = new string[] { "moveSpeed", "walkSpeed", "sprintSpeed", "speed", "wanderSpeed" };
            foreach (var n in names)
            {
                var f = t.GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null && f.FieldType == typeof(float))
                {
                    float old = (float)f.GetValue(comp);
                    f.SetValue(comp, old * speedMultiplier);
                }
                var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null && p.PropertyType == typeof(float) && p.CanRead && p.CanWrite)
                {
                    float old = (float)p.GetValue(comp, null);
                    p.SetValue(comp, old * speedMultiplier, null);
                }
            }
        }

        // 3) Try Rigidbody velocity scaling as a last resort
        var rbs = hallucinationInstance.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbs)
        {
            rb.velocity *= speedMultiplier;
        }

        speedupApplied = true;
        Debug.Log($"[Level3Controller] Applied x{speedMultiplier} speedup to hallucination at {speedupTime} seconds.");
    }
}
