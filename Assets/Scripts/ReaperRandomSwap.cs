using System.Collections;
using UnityEngine;

/// <summary>
/// Randomly swaps an NPC's visible body to a reaper variant for a short time.
/// No speed/animation trigger—pure random intervals and durations.
/// Assign normalBody and reaperBody children in the Inspector.
/// </summary>
public class ReaperRandomSwap : MonoBehaviour
{
    [Header("Bodies")]
    public GameObject normalBody;
    public GameObject reaperBody;

    [Header("Timings (seconds)")]
    [Tooltip("Random time between swap attempts.")]
    public Vector2 intervalRange = new Vector2(8f, 15f);

    [Tooltip("Random duration to stay as the reaper.")]
    public Vector2 durationRange = new Vector2(0.5f, 1.5f);

    [Header("Chance")]
    [Tooltip("Chance to swap to reaper on each attempt (0-1).")]
    [Range(0f, 1f)] public float swapChance = 0.5f;

    private Coroutine routine;

    void OnEnable()
    {
        if (normalBody == gameObject)
        {
            Debug.LogWarning("[ReaperRandomSwap] normalBody cannot be the same GameObject as this script. Assign a child mesh object instead.");
            return;
        }
        if (reaperBody == gameObject)
        {
            Debug.LogWarning("[ReaperRandomSwap] reaperBody cannot be the same GameObject as this script. Assign a child mesh object instead.");
            reaperBody = null;
        }

        // Ensure normal body is visible by default
        if (normalBody) normalBody.SetActive(true);
        if (reaperBody) reaperBody.SetActive(false);

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SwapLoop());
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;

        // Ensure normal is on when disabled
        if (normalBody && normalBody != gameObject) normalBody.SetActive(true);
        if (reaperBody && reaperBody != gameObject) reaperBody.SetActive(false);
    }

    IEnumerator SwapLoop()
    {
        while (true)
        {
            float wait = Random.Range(intervalRange.x, intervalRange.y);
            yield return new WaitForSeconds(wait);

            // Skip if no reaper body assigned
            if (reaperBody == null)
            {
                // keep normal visible if reaper is missing
                if (normalBody && !normalBody.activeSelf)
                    normalBody.SetActive(true);
                continue;
            }

            if (Random.value <= swapChance)
            {
                if (reaperBody) reaperBody.SetActive(true);
                if (normalBody) normalBody.SetActive(false);

                float dur = Random.Range(durationRange.x, durationRange.y);
                yield return new WaitForSeconds(dur);

                if (reaperBody) reaperBody.SetActive(false);
                if (normalBody) normalBody.SetActive(true);
            }
            else
            {
                // ensure normal stays on between failed rolls
                if (normalBody && !normalBody.activeSelf)
                    normalBody.SetActive(true);
            }
        }
    }
}

