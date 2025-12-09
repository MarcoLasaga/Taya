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
    public Vector2 durationRange = new Vector2(0.5f, 1.5f);

    [Header("Chance")]
    [Tooltip("Chance (0-1) to perform a swap on each attempt.")]
    [Range(0f, 1f)] public float swapChance = 0.5f;

    private Coroutine routine;

    void OnEnable()
    {
        if (characterNormal == gameObject)
        {
            Debug.LogWarning("[CharacterSwapRandom] characterNormal cannot be the same GameObject as this script. Assign a child mesh object instead.");
            return;
        }
        if (characterAlternate == gameObject)
        {
            Debug.LogWarning("[CharacterSwapRandom] characterAlternate cannot be the same GameObject as this script. Assign a child mesh object instead.");
            characterAlternate = null;
        }

        SetActiveState(normalOn: true);
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SwapLoop());
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        SetActiveState(normalOn: true); // revert to normal
    }

    IEnumerator SwapLoop()
    {
        while (true)
        {
            float wait = Random.Range(intervalRange.x, intervalRange.y);
            yield return new WaitForSeconds(wait);

            if (characterAlternate == null)
            {
                SetActiveState(normalOn: true);
                continue;
            }

            if (Random.value <= swapChance)
            {
                // Align pose before showing alternate
                CopyPose(from: characterNormal, to: characterAlternate);

                SetActiveState(normalOn: false);

                float dur = Random.Range(durationRange.x, durationRange.y);
                yield return new WaitForSeconds(dur);

                // Align pose back before showing normal
                CopyPose(from: characterAlternate, to: characterNormal);
                SetActiveState(normalOn: true);
            }
            else
            {
                SetActiveState(normalOn: true);
            }
        }
    }

    void SetActiveState(bool normalOn)
    {
        if (characterNormal) characterNormal.SetActive(normalOn);
        if (characterAlternate) characterAlternate.SetActive(!normalOn);
    }

    void CopyPose(GameObject from, GameObject to)
    {
        if (from == null || to == null) return;
        to.transform.SetPositionAndRotation(from.transform.position, from.transform.rotation);
    }
}

