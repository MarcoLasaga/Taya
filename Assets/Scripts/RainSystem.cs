using UnityEngine;

/// <summary>
/// RainSystem controls a particle system for rain and optional ambient rain audio.
/// Attach this script to a GameObject with a ParticleSystem component.
/// </summary>
public class RainSystem : MonoBehaviour
{
    [Header("Particle System")]
    [Tooltip("The particle system for rain. If null, will try to auto-find on this GameObject.")]
    public ParticleSystem rainParticles;

    [Header("Audio")]
    [Tooltip("AudioSource for ambient rain sound loop")]
    public AudioSource rainAudioSource;

    [Tooltip("Audio clip for rain ambient sound (typically a looping recording)")]
    public AudioClip rainAudioClip;

    [Tooltip("Volume for the rain audio (0-1)")]
    [Range(0f, 1f)]
    public float rainAudioVolume = 0.5f;

    [Header("Control")]
    [Tooltip("Whether rain is currently active")]
    public bool isRaining = true;

    void Start()
    {
        // auto-find particle system if not assigned
        if (rainParticles == null)
            rainParticles = GetComponent<ParticleSystem>();

        // auto-find or create AudioSource if not assigned
        if (rainAudioSource == null)
            rainAudioSource = GetComponent<AudioSource>();

        if (rainAudioSource == null)
        {
            rainAudioSource = gameObject.AddComponent<AudioSource>();
            rainAudioSource.loop = true;
            rainAudioSource.spatialBlend = 0f; // make it non-spatial (ambient)
            rainAudioSource.volume = rainAudioVolume;
            Debug.Log("[RainSystem] Created AudioSource on " + gameObject.name);
        }

        // set up the audio source
        rainAudioSource.loop = true;
        rainAudioSource.spatialBlend = 0f; // ambient sound (global)
        rainAudioSource.volume = rainAudioVolume;

        // start rain if enabled
        if (isRaining)
            StartRain();
        else
            StopRain();
    }

    public void StartRain()
    {
        isRaining = true;

        if (rainParticles != null && !rainParticles.isPlaying)
            rainParticles.Play();

        if (rainAudioSource != null && rainAudioClip != null && !rainAudioSource.isPlaying)
        {
            rainAudioSource.clip = rainAudioClip;
            rainAudioSource.Play();
        }

        Debug.Log("[RainSystem] Rain started");
    }

    public void StopRain()
    {
        isRaining = false;

        if (rainParticles != null && rainParticles.isPlaying)
            rainParticles.Stop();

        if (rainAudioSource != null && rainAudioSource.isPlaying)
            rainAudioSource.Stop();

        Debug.Log("[RainSystem] Rain stopped");
    }

    public void SetRainAudioVolume(float volume)
    {
        rainAudioVolume = Mathf.Clamp01(volume);
        if (rainAudioSource != null)
            rainAudioSource.volume = rainAudioVolume;
    }

    void OnValidate()
    {
        // keep volume synced in inspector
        if (rainAudioSource != null)
            rainAudioSource.volume = rainAudioVolume;
    }
}
