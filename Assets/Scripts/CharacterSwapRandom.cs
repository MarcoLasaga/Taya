using System.Collections;
using UnityEngine;

/// <summary>
/// Randomly swaps an NPC between two character variants (e.g., normal and reaper)
/// based on probability and random timing. No player input required.
/// </summary>
public class CharacterSwapRandom : MonoBehaviour
{
    [Header("Characters")]
    public GameObject characterNormal;
    public GameObject characterAlternate;

    [Header("Timings (seconds)")]
    [Tooltip("Random time between swap attempts.")]
    public Vector2 intervalRange = new Vector2(8f, 15f);

    [Tooltip("Random duration to stay as the alternate character when a swap occurs.")]
    public Vector2 durationRange = new Vector2(5f, 5f);

    [Header("Chance")]
    [Tooltip("Chance (0-1) to perform a swap on each attempt.")]
    [Range(0f, 1f)] public float swapChance = 0.5f;

    [Header("Movement")]
    [Tooltip("The speed of the character when in the alternate (Grim Reaper) form.")]
    public float grimReaperSpeed = 7f;

    private Coroutine routine;
    private NPCStateMachine npcStateMachine;

    void OnEnable()
    {
        Debug.Log($"[CharacterSwapRandom] {gameObject.name}: OnEnable called.");

        // This check is crucial. If the user assigns the root object to characterNormal,
        // the script will abort and nothing will work.
        if (characterNormal == gameObject)
        {
            Debug.LogError($"[CharacterSwapRandom] {gameObject.name}: 'Character Normal' is assigned to the root GameObject. It must be a child object. Aborting initialization.", gameObject);
            return;
        }
        if (characterAlternate == gameObject)
        {
            Debug.LogError($"[CharacterSwapRandom] {gameObject.name}: 'Character Alternate' is assigned to the root GameObject. It must be a child object. Aborting initialization.", gameObject);
            return;
        }

        npcStateMachine = GetComponent<NPCStateMachine>();
        if (npcStateMachine == null)
        {
             Debug.LogError($"[CharacterSwapRandom] {gameObject.name}: NPCStateMachine component not found! The 'powered movement' feature will not work.", gameObject);
        }

        SetActiveState(normalOn: true);
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SwapLoop());
        Debug.Log($"[CharacterSwapRandom] {gameObject.name}: Initialization complete, starting SwapLoop.");
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        // The call to SetActiveState was removed from here to prevent the 
        // "GameObject is already being activated or deactivated" error.
    }

    IEnumerator SwapLoop()
    {
        Debug.Log($"[CharacterSwapRandom] {gameObject.name}: Starting SwapLoop.");
        while (true)
        {
            float wait = Random.Range(intervalRange.x, intervalRange.y);
            yield return new WaitForSeconds(wait);

            if (characterAlternate == null)
            {
                Debug.LogWarning($"[CharacterSwapRandom] {gameObject.name}: characterAlternate is not assigned. Cannot swap.");
                SetActiveState(normalOn: true);
                continue;
            }

            float randomValue = Random.value;
            Debug.Log($"[CharacterSwapRandom] {gameObject.name}: Attempting swap. (Random value: {randomValue}, Swap chance: {swapChance})");

            if (randomValue <= swapChance)
            {
                Debug.Log($"[CharacterSwapRandom] {gameObject.name}: Swap chance SUCCEEDED. Beginning swap logic...");

                // Align pose before showing alternate
                Debug.Log($"[CharacterSwapRandom] {gameObject.name}: STEP 1: Calling CopyPose...");
                CopyPose(from: characterNormal, to: characterAlternate);
                Debug.Log($"[CharacterSwapRandom] {gameObject.name}: STEP 1: CopyPose finished.");

                Debug.Log($"[CharacterSwapRandom] {gameObject.name}: STEP 2: Calling SetActiveState(normalOn: false)...");
                SetActiveState(normalOn: false);
                Debug.Log($"[CharacterSwapRandom] {gameObject.name}: STEP 2: SetActiveState finished.");
                
                if (characterNormal != null && characterNormal.activeInHierarchy)
                {
                    Debug.LogError($"[CharacterSwapRandom] {gameObject.name}: ERROR: characterNormal is still active after SetActiveState(false) call!");
                }

                if (npcStateMachine != null)
                {
                    Debug.Log($"[CharacterSwapRandom] {gameObject.name}: Forcing SetRunningAnimation on NPCStateMachine for Grim Reaper.");
                    npcStateMachine.SetRunningAnimation();
                }
                
                Debug.Log($"[CharacterSwapRandom] Successfully hallucinated {gameObject.name} into Grim Reaper form!");

                float dur = Random.Range(durationRange.x, durationRange.y);
                
                if (npcStateMachine != null)
                {
                    Debug.Log($"[CharacterSwapRandom] {gameObject.name}: STEP 3: Calling ApplyTemporarySpeed for {dur} seconds...");
                    // This now calls the new signature, passing "this" (the CharacterSwapRandom instance)
                    // as the coroutine runner. This is the fix for the "coroutine on inactive object" bug.
                    npcStateMachine.ApplyTemporarySpeed(this, grimReaperSpeed, dur);
                    Debug.Log($"[CharacterSwapRandom] {gameObject.name}: STEP 3: ApplyTemporarySpeed finished.");
                }

                Debug.Log($"[CharacterSwapRandom] {gameObject.name}: STEP 4: Waiting for {dur} seconds...");
                yield return new WaitForSeconds(dur);
                Debug.Log($"[CharacterSwapRandom] {gameObject.name}: STEP 4: Wait finished. Swapping back...");

                // Align pose back before showing normal
                CopyPose(from: characterAlternate, to: characterNormal);
                SetActiveState(normalOn: true);
                Debug.Log($"[CharacterSwapRandom] {gameObject.name}: Swap back complete.");
            }
            else
            {
                Debug.Log($"[CharacterSwapRandom] {gameObject.name}: Swap chance failed. Staying normal.");
                SetActiveState(normalOn: true);
            }
        }
    }

    void SetActiveState(bool normalOn)
    {
        // This check prevents errors when trying to set active state while the root object is already being deactivated.
        if (!gameObject.activeInHierarchy) return;

        if (normalOn)
        {
            if (characterNormal) characterNormal.SetActive(true);
            if (characterAlternate) characterAlternate.SetActive(false);
        }
        else
        {
            if (characterNormal) characterNormal.SetActive(false);
            if (characterAlternate) characterAlternate.SetActive(true);
        }
    }

    void CopyPose(GameObject from, GameObject to)
    {
        if (from == null || to == null) return;
        to.transform.SetPositionAndRotation(from.transform.position, from.transform.rotation);
    }
}