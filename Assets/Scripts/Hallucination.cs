using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hallucination : MonoBehaviour
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
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SwapLoop());
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
    }

    IEnumerator SwapLoop()
    {
        while (true)
        {
            float wait = Random.Range(intervalRange.x, intervalRange.y);
            yield return new WaitForSeconds(wait);

            // Skip if no reaper body assigned
            if (reaperBody == null)
                continue;

            if (Random.value <= swapChance)
            {
                if (reaperBody) reaperBody.SetActive(true);
                if (normalBody) normalBody.SetActive(false);

                float dur = Random.Range(durationRange.x, durationRange.y);
                yield return new WaitForSeconds(dur);

                if (reaperBody) reaperBody.SetActive(false);
                if (normalBody) normalBody.SetActive(true);
            }
        }
    }
}

