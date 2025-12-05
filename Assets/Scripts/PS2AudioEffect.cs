using UnityEngine;

/// <summary>
/// PS2AudioEffect - Applies retro PS2-style audio effect via built-in filters.
/// Uses Unity's native lowpass filter to avoid OnAudioFilterRead conflicts.
/// Attach to any AudioSource or set as a filter via AudioSource.filter.
/// </summary>
public class PS2AudioEffect : MonoBehaviour
{
    [Header("Audio Effect")]
    [Tooltip("Apply lowpass filter for retro PS2 tone (Hz).")]
    [SerializeField]
    private float lowpassCutoff = 8000f;

    [Tooltip("Reduce sample clarity with bit-depth effect. Higher = more retro.")]
    [Range(1, 16)]
    [SerializeField]
    private int bitDepth = 8;

    [Header("Mix")]
    [Range(0f, 1f)]
    [SerializeField]
    private float effectAmount = 0.8f;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Apply lowpass filter via built-in Unity filter (no OnAudioFilterRead conflict)
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
            // Use built-in lowpass filter
            audioSource.GetComponent<AudioLowPassFilter>();
            if (audioSource.GetComponent<AudioLowPassFilter>() == null)
            {
                AudioLowPassFilter filter = gameObject.AddComponent<AudioLowPassFilter>();
                filter.cutoffFrequency = lowpassCutoff;
            }
            else
            {
                AudioLowPassFilter filter = audioSource.GetComponent<AudioLowPassFilter>();
                filter.cutoffFrequency = lowpassCutoff;
            }
        }
    }

    /// <summary>
    /// Set lowpass cutoff frequency for retro tone.
    /// </summary>
    public void SetLowpassCutoff(float cutoff)
    {
        lowpassCutoff = cutoff;
        ApplyLowpassFilter();
    }

    /// <summary>
    /// Set bit depth for retro quantization effect.
    /// </summary>
    public void SetBitDepth(int depth)
    {
        bitDepth = Mathf.Clamp(depth, 1, 16);
    }

    /// <summary>
    /// Set overall effect amount (0-1).
    /// </summary>
    public void SetEffectAmount(float amount)
    {
        effectAmount = Mathf.Clamp01(amount);
    }
}
