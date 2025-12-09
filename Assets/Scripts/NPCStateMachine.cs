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
    public Animation legacyAnimation;
    public AudioSource audioSource;

    [Header("Settings")]
    public float wanderRange = 6f;
    public float wanderSpeed = 2f;
    public float tayaSpeed = 5f;
    public float escapeSpeed = 4f;

    [Header("Animation Clips")]
    [Tooltip("Animation clip to play when NPC is running/moving")]
    public AnimationClip runningAnimation;
    [Tooltip("Animation clip to play when NPC is standing/idle")]
    public AnimationClip standingAnimation;

    [HideInInspector] public GameManager GM;

    [Header("Debug Tag Speeds")]
    public bool debugTagSpeeds = false;
    public float debugTayaSpeedOnTag = 15f;
    public float debugEscapeeSpeedOnTag = 0.5f;
    public float debugTagDuration = 2f;

    private Coroutine speedCoroutine = null;
    private float previousSpeed = 0f;

    public float tagTimeout = 2f;
    [HideInInspector] public GameObject chaseTarget = null;
    [HideInInspector] public float chaseTimer = 0f;

    // === FSM START ===
    public BaseState currentState;

    public IdleState idleState = new IdleState();
    public WanderState wanderState = new WanderState();
    public ChaseState chaseState = new ChaseState();
    public EscapeState escapeState = new EscapeState();
    public TayaState tayaState = new TayaState();

    private int isRunningHash = Animator.StringToHash("isRunning");
    private int isIdleHash = Animator.StringToHash("isIdle");
    // === FSM END ===

    void Start()
    {
        GM = FindObjectOfType<GameManager>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        legacyAnimation = GetComponent<Animation>();
        audioSource = GetComponent<AudioSource>();

        if (agent != null)
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            agent.stoppingDistance = 0f;
            agent.autoBraking = false;
        }

        SwitchState(idleState);
    }

    public void SetRunningAnimation()
    {
        Debug.Log($"[NPCStateMachine] SetRunningAnimation called for {name}. runningAnimation = {(runningAnimation != null ? runningAnimation.name : "NULL")}");

        if (runningAnimation != null)
        {
            if (legacyAnimation != null)
            {
                Debug.Log($"[NPCStateMachine] Playing running animation '{runningAnimation.name}' via legacy Animation");
                legacyAnimation.CrossFade(runningAnimation.name, 0.3f);
            }
            else if (anim != null)
            {
                Debug.Log($"[NPCStateMachine] Playing running animation via Animator (isRunning=true, isIdle=false)");
                anim.SetBool(isRunningHash, true);
                anim.SetBool(isIdleHash, false);
            }
            else
            {
                Debug.LogWarning($"[NPCStateMachine] No Animation or Animator component found on {name}!");
            }
        }
        else
        {
            Debug.LogWarning($"[NPCStateMachine] runningAnimation is not assigned on {name}");
        }
    }

    public void SetIdleAnimation()
    {
        Debug.Log($"[NPCStateMachine] SetIdleAnimation called for {name}. standingAnimation = {(standingAnimation != null ? standingAnimation.name : "NULL")}");

        if (standingAnimation != null)
        {
            if (legacyAnimation != null)
            {
                Debug.Log($"[NPCStateMachine] Playing idle animation '{standingAnimation.name}' via legacy Animation");
                legacyAnimation.CrossFade(standingAnimation.name, 0.3f);
            }
            else if (anim != null)
            {
                Debug.Log($"[NPCStateMachine] Playing idle animation via Animator (isRunning=false, isIdle=true)");
                anim.SetBool(isRunningHash, false);
                anim.SetBool(isIdleHash, true);
            }
            else
            {
                Debug.LogWarning($"[NPCStateMachine] No Animation or Animator component found on {name}!");
            }
        }
        else
        {
            Debug.LogWarning($"[NPCStateMachine] standingAnimation is not assigned on {name}");
        }
    }

    void Update()
    {
        if (!GM.gameRunning) return;

        currentState.UpdateState(this);

        // Update Animator Speed similar to player controller logic
        Vector3 moveInput = agent != null ? agent.velocity : Vector3.zero;
        UpdateAnimatorSpeed(moveInput);
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

        // Only the current Taya can transfer the tag
        if (!isTaya || gameObject.CompareTag("Hallucination"))
            return;

        // Prevent redundant swaps: don't swap if the collision target is already the Taya
        if (col.gameObject == GM.currentTaya)
            return;

        // Allow NPC-to-player, NPC-to-NPC, and player-to-NPC swaps
        if (col.gameObject.CompareTag("Player"))
        {
            GM.SwapTaya(col.gameObject);
        }
        else
        {
            NPCStateMachine f = col.gameObject.GetComponent<NPCStateMachine>();
            if (f != null)
            {
                GM.SwapTaya(f.gameObject);

                if (debugTagSpeeds)
                {
                    f.ApplyTemporarySpeed(debugEscapeeSpeedOnTag, debugTagDuration);
                    this.ApplyTemporarySpeed(debugTayaSpeedOnTag, debugTagDuration);
                }
            }
        }
    }

    public void ApplyTemporarySpeed(float speed, float duration)
    {
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

        if (agent != null)
        {
            if (currentState == tayaState)
                agent.speed = tayaSpeed;
            else
                agent.speed = wanderSpeed;
        }

        speedCoroutine = null;
    }

    void UpdateAnimatorSpeed(Vector3 moveInput)
    {
        if (anim == null)
            return;

        if (moveInput == Vector3.zero)
            anim.SetFloat("Speed", 0f);
        else if (!Input.GetKey(KeyCode.LeftShift))
            anim.SetFloat("Speed", wanderSpeed);
        else
            anim.SetFloat("Speed", tayaSpeed);
    }
}
