using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Level 2 adjustments:
/// - Sets game duration (default 300s = 5 minutes)
/// - Makes NPCs faster (multiplier)
/// - Makes player slower (multipliers for walk/sprint)
/// - Increases jump and dash cooldowns (longer timers)
///
/// Attach this to a GameObject in your Level 2 scene (for example the same GameObject as GameManager).
/// </summary>
[DefaultExecutionOrder(-100)] // apply duration before GameManager.Start runs
public class Level2GameManager : MonoBehaviour
{
    [Header("Level 2 Settings")]
    [Tooltip("Level duration in seconds (set to 300 for 5 minutes).")]
    public float levelDuration = 300f;

    [Tooltip("Multiply NPC speeds by this ( >1 makes enemies faster )")]
    public float enemySpeedMultiplier = 1.4f;

    [Tooltip("Multiply player walk speed by this ( <1 makes player slower )")]
    public float playerWalkMultiplier = 0.7f;

    [Tooltip("Multiply player sprint speed by this ( <1 makes sprint slower )")]
    public float playerSprintMultiplier = 0.8f;

    [Tooltip("Multiply dash cooldown by this ( >1 makes dash cooldown longer )")]
    public float dashCooldownMultiplier = 1.5f;

    [Tooltip("Multiply jump cooldown by this ( >1 makes jump cooldown longer )")]
    public float jumpCooldownMultiplier = 1.5f;

    void Awake()
    {
        // Only apply overrides in the Level2 scene to avoid changing other levels
        var sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Level2"))
            return;

        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.gameDuration = levelDuration;
            Debug.Log($"[Level2GameManager] Set GameManager.gameDuration = {levelDuration}s");
        }
        else
        {
            Debug.LogWarning("[Level2GameManager] GameManager not found in scene (Awake).");
        }
    }

    void Start()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Level2"))
            return;

        ApplyPlayerAdjustments();
        ApplyEnemyAdjustments();
    }

    void ApplyPlayerAdjustments()
    {
        var gm = FindObjectOfType<GameManager>();
        PlayerControllerWithCamera pc = null;

        if (gm != null && gm.player != null)
            pc = gm.player.GetComponent<PlayerControllerWithCamera>();

        if (pc == null)
            pc = FindObjectOfType<PlayerControllerWithCamera>();

        if (pc == null)
        {
            Debug.LogWarning("[Level2GameManager] PlayerControllerWithCamera not found; skipping player adjustment.");
            return;
        }

        // Apply slower movement
        pc.walkSpeed *= playerWalkMultiplier;
        pc.sprintSpeed *= playerSprintMultiplier;

        // Extend cooldowns
        pc.jumpCooldown *= jumpCooldownMultiplier;
        pc.dashCooldown *= dashCooldownMultiplier;

        Debug.Log($"[Level2GameManager] Player adjusted: walk={pc.walkSpeed:F2}, sprint={pc.sprintSpeed:F2}, jumpCooldown={pc.jumpCooldown:F2}, dashCooldown={pc.dashCooldown:F2}");
    }

    void ApplyEnemyAdjustments()
    {
        var gm = FindObjectOfType<GameManager>();
        NPCStateMachine[] npcs;

        if (gm != null && gm.friends != null && gm.friends.Count > 0)
        {
            // Prefer configured friend list from GameManager
            var list = new System.Collections.Generic.List<NPCStateMachine>();
            foreach (var f in gm.friends)
            {
                if (f == null) continue;
                var sm = f.GetComponent<NPCStateMachine>();
                if (sm != null) list.Add(sm);
            }
            npcs = list.ToArray();
        }
        else
        {
            // Fallback: find all NPCStateMachine instances in scene
            npcs = FindObjectsOfType<NPCStateMachine>();
        }

        if (npcs == null || npcs.Length == 0)
        {
            Debug.LogWarning("[Level2GameManager] No NPCStateMachine instances found; skipping enemy adjustment.");
            return;
        }

        int updated = 0;
        foreach (var sm in npcs)
        {
            if (sm == null) continue;

            // Increase configured speeds
            sm.wanderSpeed *= enemySpeedMultiplier;
            sm.tayaSpeed *= enemySpeedMultiplier;
            sm.escapeSpeed *= enemySpeedMultiplier;

            // Update NavMeshAgent current speed to reflect new values
            if (sm.agent != null)
            {
                if (sm.currentState == sm.tayaState)
                    sm.agent.speed = sm.tayaSpeed;
                else
                    sm.agent.speed = sm.wanderSpeed;
            }

            updated++;
        }

        Debug.Log($"[Level2GameManager] Adjusted {updated} NPCs with enemySpeedMultiplier={enemySpeedMultiplier:F2}");
    }
}