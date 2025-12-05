using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCStateMachine : MonoBehaviour
{
    [Header("Status")]
    public bool isTaya = false;

    [Header("Components")]
    public NavMeshAgent agent;
    public Animator anim;
    public AudioSource audioSource;

    [Header("Settings")]
    public float wanderRange = 6f;
    public float wanderSpeed = 2f;
    public float tayaSpeed = 5f;
    public float escapeSpeed = 4f;

    [HideInInspector] public GameManager GM;
    // Debug tagging speed overrides (for debugging only)
    [Header("Debug Tag Speeds")]
    public bool debugTagSpeeds = false;
    public float debugTayaSpeedOnTag = 15f;
    public float debugEscapeeSpeedOnTag = 0.5f;
    public float debugTagDuration = 2f;

    // internal for temporary speed overrides
    private Coroutine speedCoroutine = null;
    private float previousSpeed = 0f;
    // Tag/chase helpers: how long to attempt tagging the current target
    // before switching to a different target (in seconds).
    public float tagTimeout = 2f;
    [HideInInspector] public GameObject chaseTarget = null;
    [HideInInspector] public float chaseTimer = 0f;

    // FSM
    public BaseState currentState;

    public IdleState idleState = new IdleState();
    public WanderState wanderState = new WanderState();
    public ChaseState chaseState = new ChaseState();
    public EscapeState escapeState = new EscapeState();
    public TayaState tayaState = new TayaState();

    void Start()
    {
        GM = FindObjectOfType<GameManager>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Remove agent avoidance and stopping space so NPCs can contact each other.
        // This disables NavMeshAgent obstacle avoidance (agents won't steer
        // away from each other) and reduces stopping distance/radius so
        // they can come into collider contact.
        if (agent != null)
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            agent.stoppingDistance = 0f;
            // shrink the agent radius so agents don't keep a buffer between them
            agent.radius = 0.1f;
            // reduce auto-braking so agents don't slow down early
            agent.autoBraking = false;
        }

        SwitchState(idleState);
    }

    void Update()
    {
        if (!GM.gameRunning) return;

        currentState.UpdateState(this);
    }

    public void SwitchState(BaseState newState)
    {
        if (currentState != null)
            currentState.ExitState(this);

        currentState = newState;
        newState.EnterState(this);
    }

    private void OnCollisionEnter(Collision col)
    {
        if (!GM.gameRunning) return;

        if (isTaya)
        {
            if (col.gameObject.CompareTag("Player"))
                GM.SwapTaya(col.gameObject);

            NPCStateMachine f = col.gameObject.GetComponent<NPCStateMachine>();
            if (f != null)
            {
                GM.SwapTaya(f.gameObject);

                // Debug behavior: when a Taya tags another NPC, temporarily
                // make the Taya extremely fast and the tagged NPC very slow
                // to make the swap visible for debugging.
                if (debugTagSpeeds)
                {
                    // slow the tagged NPC
                    f.ApplyTemporarySpeed(debugEscapeeSpeedOnTag, debugTagDuration);
                    // speed up the current (this) Taya
                    this.ApplyTemporarySpeed(debugTayaSpeedOnTag, debugTagDuration);
                }
            }
        }
    }

    // Applies a temporary speed to this NPC's NavMeshAgent for a duration,
    // then restores an appropriate speed depending on current state.
    public void ApplyTemporarySpeed(float speed, float duration)
    {
        // stop any existing temp speed coroutine
        if (speedCoroutine != null) StopCoroutine(speedCoroutine);
        previousSpeed = agent != null ? agent.speed : 0f;
        speedCoroutine = StartCoroutine(TemporarySpeedCoroutine(speed, duration));
    }

    private IEnumerator TemporarySpeedCoroutine(float speed, float duration)
    {
        if (agent != null) agent.speed = speed;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // restore speed according to state (prefer role-specific speeds)
        if (agent != null)
        {
            if (currentState == tayaState)
                agent.speed = tayaSpeed;
            else
                agent.speed = wanderSpeed;
        }

        speedCoroutine = null;
    }
}
