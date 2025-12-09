using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Primary player GameObject.")]
    public GameObject player;

    [Tooltip("List of NPC friends (kid1, kid2, kid3, kid4, kid5).")]
    public List<GameObject> friends;
    public List<GameObject> allFriends => friends;

    [Header("UI")]
    public TextMeshProUGUI gameMessageText;
    public TextMeshProUGUI tayaNameText;
    public TextMeshProUGUI timerText;
    // optional subtitle text placed on the Canvas and named e.g. 'subtitles'
    public TextMeshProUGUI subtitlesText;

    [Header("Game Settings")]
    public float gameDuration = 300f;

    [HideInInspector] public bool gameRunning = false;
    [HideInInspector] public bool gameUnlocked = false;

    [HideInInspector] public GameObject currentTaya;

    [Header("Music")]
    public AudioSource bgMusic;
    [Header("Intro / Outro Voice Clips")]
    public AudioClip introVoiceClip;
    public string introSubtitle = "";
    public AudioClip postEndVoiceClip;
    [Header("Environment")]
    [Tooltip("Optional: DayNightCycle controller to start/stop with the game")]
    public DayNightCycle dayNightCycle;
    [Header("End Sequence")]
    public AudioSource voiceSource;
    public AudioClip endVoiceClip;
    // When true the game is waiting for the player to interact with the
    // StartTrigger cube to finish the scene and move to the next scene.
    [HideInInspector] public bool awaitingEndInteraction = false;
    [Header("Swap")]
    [Tooltip("Minimum seconds between Taya swaps to avoid rapid toggles")]
    public float swapCooldown = 0.5f;
    [Header("Start/End Cube Options")]
    [Tooltip("If true, stepping on the start cube will immediately start the game (no Y press needed)")]
    public bool startCubeAutoStart = false;
    private float lastSwapTime = -10f;
    [Tooltip("Show on-screen debug HUD with current Taya and swap info")]
    public bool showDebugUI = true;

    private float remainingTime;
    // Expose remaining time for external scripts (read-only)
    public float RemainingTime { get { return remainingTime; } }
    // Player speed slow state (applied when 60s left)
    private bool playerSpeedReduced = false;
    private float savedWalkSpeed = -1f;
    private float savedSprintSpeed = -1f;

    void Start()
    {
        remainingTime = gameDuration;

        // Initial menu UI: keep menu text hidden (clean output per design)
        if (gameMessageText) { gameMessageText.text = string.Empty; gameMessageText.gameObject.SetActive(false); }
        if (tayaNameText) { tayaNameText.text = "Taya: None"; tayaNameText.gameObject.SetActive(false); }
        if (timerText) { timerText.text = "Time: 05:00"; timerText.gameObject.SetActive(false); }

        if (subtitlesText)
        {
            subtitlesText.gameObject.SetActive(false);
            subtitlesText.text = string.Empty;
        }

        // Play intro voicecutscene if supplied, then hide the menu so player can explore the scene
        if (voiceSource != null && introVoiceClip != null)
        {
            StartCoroutine(PlayVoiceAndHideMenu(introVoiceClip, introSubtitle));
        }

        // Ensure the friends list points to the new kid1–kid5 GameObjects
        AutoPopulateFriendsList();

        // Ensure cooldown UI hidden until player triggers the start cube
        if (player != null)
        {
            var pc = player.GetComponent<PlayerControllerWithCamera>();
            if (pc != null)
                pc.HideCooldownUI();
        }

        // start the Day/Night cycle as soon as the scene loads (if assigned)
        if (dayNightCycle != null)
        {
            dayNightCycle.StartCycle();
        }
    }

    // Populate friends list with named taggers (Marcus, Malone, Bella, Totoy, Cornbeef)
    void AutoPopulateFriendsList()
    {
        if (friends == null)
            friends = new List<GameObject>();
        else
            friends.Clear();

        string[] names = { "Marcus", "Malone", "Vella", "Totoy", "Cornbeef, Grim Reaper" };
        foreach (var n in names)
        {
            var go = GameObject.Find(n);
            if (go != null)
                friends.Add(go);
        }
    }

    void Update()
    {
        // Toggle debug HUD at runtime
        if (Input.GetKeyDown(KeyCode.F))
        {
            showDebugUI = !showDebugUI;
            Debug.Log($"[GameManager] showDebugUI set to {showDebugUI}");
        }

        if (!gameUnlocked) return;

        if (!gameRunning && Input.GetKeyDown(KeyCode.Y))
        {
            if (bgMusic != null && !bgMusic.isPlaying) bgMusic.Play();
            StartGame();
            return;
        }

        if (gameRunning)
        {
            // Ensure debug HUD stays visible during gameplay
            showDebugUI = true;
            UpdateTimer();
        }
    }

    public void UnlockGame()
    {
        // Called when player steps on the start box. Show a subtitle prompt
        gameUnlocked = true;
        if (subtitlesText != null)
        {
            subtitlesText.text = "Do you want to play? Press Y to start";
            subtitlesText.gameObject.SetActive(true);
        }
        // Also show the main game message text so the prompt appears like other UI
        if (gameMessageText != null)
        {
            gameMessageText.text = "Press Y to start";
            gameMessageText.gameObject.SetActive(true);
        }
    }

    // Called by a start-cube trigger when the player steps on it.
    public void OnStartCubeTriggered(GameObject interactor)
    {
        // unlock the game (shows the prompt)
        UnlockGame();

        // reveal cooldown UI on the player's controller
        PlayerControllerWithCamera pc = null;
        if (interactor != null)
            pc = interactor.GetComponent<PlayerControllerWithCamera>();
        if (pc == null && player != null)
            pc = player.GetComponent<PlayerControllerWithCamera>();
        if (pc != null)
            pc.ShowCooldownUI();

        // optionally auto-start the game immediately when stepping on the cube
        if (startCubeAutoStart && !gameRunning)
        {
            // hide the subtitle prompt if present
            if (subtitlesText != null)
            {
                subtitlesText.gameObject.SetActive(false);
                subtitlesText.text = string.Empty;
            }
            StartGame();
        }
    }

    // Called by an end-cube trigger when the player steps on it after the game ends.
    public void OnEndCubeTriggered(GameObject interactor)
    {
        if (!awaitingEndInteraction) return;

        // proceed to next scene or play post-end voice
        ProceedToNextScene();
    }

    void StartGame()
    {
        // Pick Taya before the game starts
        if (currentTaya == null)
            PickInitialTaya();

        gameRunning = true;
        remainingTime = gameDuration;
        showDebugUI = true;

        // Show gameplay UI
        if (gameMessageText) gameMessageText.gameObject.SetActive(false);
        if (timerText) timerText.gameObject.SetActive(true);
        if (tayaNameText) tayaNameText.gameObject.SetActive(true);
        if (subtitlesText) subtitlesText.gameObject.SetActive(false);

        if (bgMusic != null && !bgMusic.isPlaying) bgMusic.Play();
    }

    void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime < 0) remainingTime = 0;

        int min = Mathf.FloorToInt(remainingTime / 60f);
        int sec = Mathf.FloorToInt(remainingTime % 60f);

        if (timerText) timerText.text = $"Time: {min:00}:{sec:00}";

        // When we reach 60 seconds remaining, apply a 50% speed reduction to the player
        if (!playerSpeedReduced && remainingTime <= 60f)
        {
            if (player != null)
            {
                var pc = player.GetComponent<PlayerControllerWithCamera>();
                if (pc != null)
                {
                    savedWalkSpeed = pc.walkSpeed;
                    savedSprintSpeed = pc.sprintSpeed;
                    pc.walkSpeed *= 0.5f;
                    pc.sprintSpeed *= 0.5f;
                    playerSpeedReduced = true;
                    Debug.Log("[GameManager] Player speed reduced by 50% at 60s remaining.");
                }
            }
        }

        if (remainingTime <= 0)
            EndGame();
    }

    void EndGame()
    {
        gameRunning = false;

        // stop game music
        if (bgMusic != null && bgMusic.isPlaying) bgMusic.Stop();

        // hide UI texts from the screen
        if (timerText) timerText.gameObject.SetActive(false);
        if (tayaNameText) tayaNameText.gameObject.SetActive(false);
        if (gameMessageText) gameMessageText.gameObject.SetActive(false);

        // Hide player's cooldown UI (jump/dash) if present
        if (player != null)
        {
            var pc = player.GetComponent<PlayerControllerWithCamera>();
            if (pc != null)
                pc.HideCooldownUI();
        }

        // Restore player speed if it was reduced
        if (playerSpeedReduced && player != null)
        {
            var pc2 = player.GetComponent<PlayerControllerWithCamera>();
            if (pc2 != null)
            {
                if (savedWalkSpeed > 0f) pc2.walkSpeed = savedWalkSpeed;
                if (savedSprintSpeed > 0f) pc2.sprintSpeed = savedSprintSpeed;
            }
            playerSpeedReduced = false;
            savedWalkSpeed = savedSprintSpeed = -1f;
        }

        // cleanup current Taya glow and reset color
        if (currentTaya != null)
        {
            var glow = currentTaya.GetComponent<TayaGlow>();
            if (glow) Destroy(glow);

            var rends = currentTaya.GetComponentsInChildren<Renderer>();
        }

        // Ensure no one remains as Taya: force all friends to wander
        if (friends != null)
        {
            foreach (var f in friends)
            {
                if (f == null) continue;
                var sm = f.GetComponent<NPCStateMachine>();
                if (sm != null)
                {
                    sm.SwitchState(sm.wanderState);
                }
            }
        }

        // clear currentTaya reference so game logic treats no one as Taya
        currentTaya = null;

        // mark that we're now waiting for the player to interact with the cube
        awaitingEndInteraction = true;

        // keep the day/night cycle running for the duration of the scene

        if (voiceSource != null && endVoiceClip != null)
        {
            voiceSource.PlayOneShot(endVoiceClip);
        }

        // show a subtitle message for a short time (non-blocking)
        if (subtitlesText != null)
        {
            StartCoroutine(ShowSubtitle("galing mo talaga junjun", 5f));
        }
    }

    // Play a voice clip and show a subtitle while it plays; then hide the main menu
    System.Collections.IEnumerator PlayVoiceAndHideMenu(AudioClip clip, string subtitle)
    {
        if (subtitlesText != null)
        {
            subtitlesText.text = subtitle;
            subtitlesText.gameObject.SetActive(true);
        }

        if (voiceSource != null && clip != null)
        {
            voiceSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
        else
        {
            // no clip, just small delay
            yield return new WaitForSeconds(1f);
        }

        if (subtitlesText != null)
        {
            subtitlesText.gameObject.SetActive(false);
            subtitlesText.text = string.Empty;
        }

        if (gameMessageText != null)
        {
            // hide the initial menu so the player sees the clean scene
            gameMessageText.gameObject.SetActive(false);
        }
    }

    // Shows a subtitle message on the assigned TextMeshProUGUI for the
    // given duration (non-flashing).
    System.Collections.IEnumerator ShowSubtitle(string message, float duration)
    {
        if (subtitlesText == null) yield break;

        subtitlesText.text = message;
        subtitlesText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        subtitlesText.gameObject.SetActive(false);
        subtitlesText.text = string.Empty;
    }

    void PickInitialTaya()
    {
        // Only choose randomly before the game starts and if none is set
        if (gameRunning || currentTaya != null)
            return;

        if (friends == null || friends.Count == 0)
        {
            Debug.LogWarning("[GM] No friends assigned.");
            return;
        }

        int index = Random.Range(0, friends.Count);
        currentTaya = friends[index];

        AddTayaGlow(currentTaya);

        if (tayaNameText) tayaNameText.text = $"Taya: {currentTaya.name}";

        // Set NPC state machines: make everyone wander, and the chosen one become Taya
        foreach (var f in friends)
        {
            if (f == null) continue;
            var sm = f.GetComponent<NPCStateMachine>();
            if (sm != null)
                sm.SwitchState(sm.wanderState);
        }

        var chosenSM = currentTaya.GetComponent<NPCStateMachine>();
        if (chosenSM != null)
            chosenSM.SwitchState(chosenSM.tayaState);
    }

    // SwapTaya preserved (existing name)
    public void SwapTaya(GameObject newTaya)
    {
        if (newTaya == null)
        {
            Debug.LogWarning("[GameManager] SwapTaya called with null");
            return;
        }
        // avoid redundant switch
        if (currentTaya == newTaya)
        {
            Debug.Log("[GameManager] SwapTaya: already current Taya");
            return;
        }

        // enforce swap cooldown
        if (Time.time - lastSwapTime < swapCooldown)
        {
            Debug.Log("[GameManager] SwapTaya: swap on cooldown");
            return;
        }


        Debug.Log($"[GameManager] Swapping Taya -> {newTaya.name}");

        // Remove old Taya glow and reset color on all known friends to avoid
        // stray tints from other scripts or materials. Use child renderers
        // where applicable.
        if (currentTaya != null)
        {
            var oldGlow = currentTaya.GetComponent<TayaGlow>();
            if (oldGlow) Destroy(oldGlow);
        }

        currentTaya = newTaya;
        lastSwapTime = Time.time;

        // Add glow on new Taya and set a neutral glow color to avoid bright red
        AddTayaGlow(newTaya);

        if (tayaNameText)
        {
            if (!tayaNameText.gameObject.activeSelf) tayaNameText.gameObject.SetActive(true);
            tayaNameText.text = $"Taya: {newTaya.name}";
        }

        Debug.Log($"[GameManager] {newTaya.name} is now the TAYA!");

        // Make all NPCs go to Wander, then set the new taya's state to TayaState
        foreach (var f in friends)
        {
            if (f == null) continue;
            var sm = f.GetComponent<NPCStateMachine>();
            if (sm != null)
                sm.SwitchState(sm.wanderState);
        }

        var newSM = newTaya.GetComponent<NPCStateMachine>();
        if (newSM != null)
            newSM.SwitchState(newSM.tayaState);
    }

    // Backwards-compatible wrapper used by other scripts (FriendAI/on-collision calls SwitchTaya)
    public void SwitchTaya(GameObject newTaya)
    {
        SwapTaya(newTaya);
    }

    // Attempts to swap Taya and returns true if the swap took place.
    public bool TrySwapTaya(GameObject newTaya)
    {
        // If the player is currently the Taya, don't allow other NPCs/friends
        // to steal the Taya role via background collisions/proximity checks.
        // This prevents unexpected swaps where the player remains 'the Taya'
        // visually but another friend becomes currentT aya.
        if (player != null && currentTaya == player)
        {
            return false;
        }

        SwapTaya(newTaya);
        return currentTaya == newTaya;
    }

    // Returns nearest non-Taya GameObject (player or friends) for Taya to chase
    public GameObject GetNearestNonTaya(Vector3 fromPosition)
    {
        GameObject nearest = null;
        float best = Mathf.Infinity;

        // check player
        if (player != null && player != currentTaya)
        {
            float d = Vector3.Distance(fromPosition, player.transform.position);
            best = d;
            nearest = player;
        }

        // check friends
        if (friends != null)
        {
            foreach (var f in friends)
            {
                if (f == null || f == currentTaya) continue;
                float d = Vector3.Distance(fromPosition, f.transform.position);
                if (d < best)
                {
                    best = d;
                    nearest = f;
                }
            }
        }

        return nearest;
    }

    // Returns the nearest non-Taya GameObject excluding the given object.
    // Useful for switching to a different target if the current one is
    // unreachable or timed out.
    public GameObject GetNearestNonTayaExcluding(Vector3 fromPosition, GameObject exclude)
    {
        GameObject nearest = null;
        float best = Mathf.Infinity;

        // check player
        if (player != null && player != currentTaya && player != exclude)
        {
            float d = Vector3.Distance(fromPosition, player.transform.position);
            best = d;
            nearest = player;
        }

        // check friends
        if (friends != null)
        {
            foreach (var f in friends)
            {
                if (f == null || f == currentTaya || f == exclude) continue;
                float d = Vector3.Distance(fromPosition, f.transform.position);
                if (d < best)
                {
                    best = d;
                    nearest = f;
                }
            }
        }

        return nearest;
    }

    // Auto-add glow component (safe: only adds if not present)
    void AddTayaGlow(GameObject taya)
    {
        if (taya == null) return;
        var glow = taya.GetComponent<TayaGlow>();
        if (glow == null)
        {
            taya.AddComponent<TayaGlow>();
        }
    }

    // Called by a trigger (StartTrigger) when the player interacts after the
    // end sequence has started. Loads the next scene (by build index + 1)
    // or, if `nextSceneName` is set, loads that scene name.
    public string nextSceneName;
    public void ProceedToNextScene()
    {
        if (!awaitingEndInteraction) return;

        awaitingEndInteraction = false;

        // If a post-end voice clip is assigned, play it with subtitle then load the next scene
        if (voiceSource != null && postEndVoiceClip != null)
        {
            StartCoroutine(PlayPostEndAndLoad(postEndVoiceClip));
            return;
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(next);
            else
                Debug.Log("[GameManager] No next scene in build settings.");
        }
    }

    System.Collections.IEnumerator PlayPostEndAndLoad(AudioClip clip)
    {
        if (subtitlesText != null)
        {
            // show the same end subtitle while the clip plays
            subtitlesText.text = "";
            subtitlesText.gameObject.SetActive(true);
        }

        voiceSource.PlayOneShot(clip);
        yield return new WaitForSeconds(clip.length);

        if (subtitlesText != null)
        {
            subtitlesText.gameObject.SetActive(false);
            subtitlesText.text = string.Empty;
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(next);
            else
                Debug.Log("[GameManager] No next scene in build settings.");
        }
    }

    // Simple on-screen debug HUD; useful when `showDebugUI` is enabled.
    void OnGUI()
    {
        if (!showDebugUI) return;

        int w = 300, h = 90;
        GUILayout.BeginArea(new Rect(10, 10, w, h));
        GUILayout.BeginVertical("box");
        GUILayout.Label($"Current Taya: {(currentTaya != null ? currentTaya.name : "None")}");
        GUILayout.Label($"Last swap: {lastSwapTime:F2} (time since: {(Time.time - lastSwapTime):F2}s)");
        GUILayout.Label($"Swap cooldown: {swapCooldown:F2}s");
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
