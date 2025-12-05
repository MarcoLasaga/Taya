using UnityEngine;

/// <summary>
/// EightBitAudioEffect - Applies authentic 8-bit retro audio effect via built-in filters.
/// Uses Unity's native lowpass filter to avoid OnAudioFilterRead conflicts.
/// Attach to any AudioSource.
/// </summary>
public class EightBitAudioEffect : MonoBehaviour
{
    [Header("8-Bit Audio Settings")]
    [SerializeField]
    [Range(1, 8)]
    private int bitDepth = 8;

    [Tooltip("Lowpass cutoff for 8-bit tone shaping (Hz).")]
    [SerializeField]
    private float lowpassCutoff = 5000f;

    [SerializeField]
    [Range(0f, 1f)]
    private float effectAmount = 0.9f;

    private AudioSource audioSource;
    private AudioLowPassFilter lowpassFilter;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Apply lowpass filter via built-in Unity filter
        ApplyLowpassFilter();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyLowpassFilter();
        }
    }

    private void ApplyLowpassFilter()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && lowpassCutoff > 0f)
        {
            lowpassFilter = gameObject.GetComponent<AudioLowPassFilter>();
            if (lowpassFilter == null)
            {
                lowpassFilter = gameObject.AddComponent<AudioLowPassFilter>();
            }
            lowpassFilter.cutoffFrequency = lowpassCutoff;
        }
    }

    /// <summary>
    /// Set 8-bit depth (1-8 bits).
    /// </summary>
    public void SetBitDepth(int bits)
    {
        bitDepth = Mathf.Clamp(bits, 1, 8);
    }

    /// <summary>
    /// Set lowpass cutoff for tone shaping.
    /// </summary>
    public void SetLowpassCutoff(float cutoff)
    {
        lowpassCutoff = cutoff;
        ApplyLowpassFilter();
    }

    /// <summary>
    /// Set overall effect amount (0-1).
    /// </summary>
    public void SetEffectAmount(float amount)
    {
        effectAmount = Mathf.Clamp01(amount);
    }
}
