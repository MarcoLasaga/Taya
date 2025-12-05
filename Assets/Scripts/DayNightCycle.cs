using UnityEngine;

[ExecuteAlways]
public class DayNightCycle : MonoBehaviour
{
    [System.Serializable]
    public class DayPhase
    {
        public string name = "Phase";
        public Color lightColor = Color.white;
        public float intensity = 1f;
        [Tooltip("Optional skybox material to use for this phase. If assigned, the cycle will try to blend between skyboxes during transitions.")]
        public Material skyboxMaterial;
        public Vector3 eulerRotation = new Vector3(50f, 30f, 0f);
        [Tooltip("Duration of the transition to this phase in seconds")]
        public float duration = 10f;
    }

    [Header("References")]
    [Tooltip("Directional light to control. If null, will try RenderSettings.sun or the first directional Light found.")]
    public Light directionalLight;

    [Header("Cycle")]
    [Tooltip("Phases that the cycle will step through in order. At least 2 recommended.")]
    public DayPhase[] phases;

    [Tooltip("Automatically start cycling on Play/Editor update")]
    public bool autoStart = true;

    [Tooltip("Loop the cycle indefinitely")]
    public bool loop = true;

    [Tooltip("Time scale factor for the cycle (1 = normal realtime)")]
    public float timeScale = 1f;

    [Header("Timing Helpers")]
    [Tooltip("If > 0 you can distribute this total seconds evenly across phases using the context menu 'Distribute Durations Equally'.")]
    public float totalCycleSeconds = 0f;

    [Tooltip("When enabled the script exposes `currentPhaseTimeLeft` which shows seconds remaining for the active transition.")]
    public bool exposePhaseTimeLeft = true;

    // read-only runtime information (seconds remaining for current transition)
    [HideInInspector]
    public float currentPhaseTimeLeft = 0f;

    [Header("Skybox / Environment")]
    [Tooltip("If true, the cycle will smoothly blend between skybox materials specified in each phase when possible.")]
    public bool blendSkybox = true;

    int currentIndex = 0;
    int nextIndex = 1;
    float timer = 0f;
    bool running = false;

    // single reusable material instance used to blend skyboxes at runtime
    Material blendedSkyboxInstance = null;

    void Reset()
    {
        // sensible defaults for an empty reset
        phases = new DayPhase[4];
        phases[0] = new DayPhase() { name = "Morning", lightColor = new Color(1f, 0.95f, 0.8f), intensity = 0.8f, eulerRotation = new Vector3(40f, 30f, 0f), duration = 10f };
        phases[1] = new DayPhase() { name = "Noon", lightColor = Color.white, intensity = 1.2f, eulerRotation = new Vector3(50f, 0f, 0f), duration = 10f };
        phases[2] = new DayPhase() { name = "Evening", lightColor = new Color(1f, 0.7f, 0.5f), intensity = 0.6f, eulerRotation = new Vector3(130f, -20f, 0f), duration = 10f };
        phases[3] = new DayPhase() { name = "Night", lightColor = new Color(0.45f, 0.55f, 1f), intensity = 0.2f, eulerRotation = new Vector3(200f, 30f, 0f), duration = 10f };
    }

    void OnValidate()
    {
        if (directionalLight == null)
        {
            if (RenderSettings.sun != null)
                directionalLight = RenderSettings.sun;
            else
            {
                var found = FindObjectOfType<Light>();
                if (found != null && found.type == LightType.Directional)
                    directionalLight = found;
            }
        }
    }

    void Start()
    {
        if (phases == null || phases.Length < 2)
        {
            Reset();
        }

        OnValidate();

        if (autoStart)
            StartCycle();
    }

    public void StartCycle()
    {
        if (phases == null || phases.Length == 0) return;
        running = true;
        currentIndex = 0;
        nextIndex = (phases.Length > 1) ? 1 : 0;
        timer = 0f;

        // apply immediate initial phase values
        ApplyPhaseInstant(phases[currentIndex]);
    }

    public void StopCycle()
    {
        running = false;
    }

    void Update()
    {
        if (!running) return;
        if (phases == null || phases.Length == 0) return;

        float dt = Time.deltaTime * Mathf.Max(0f, timeScale);
        timer += dt;

        float duration = phases[nextIndex].duration;
        if (duration <= 0f)
        {
            // instant swap
            currentIndex = nextIndex;
            nextIndex = (currentIndex + 1) % phases.Length;
            timer = 0f;
            ApplyPhaseInstant(phases[currentIndex]);
            return;
        }

        float t = Mathf.Clamp01(timer / duration);

        // update debug/readout for remaining seconds in this transition
        if (exposePhaseTimeLeft)
        {
            currentPhaseTimeLeft = Mathf.Max(0f, duration - timer);
        }

        // Lerp from current phase -> next phase
        var from = phases[currentIndex];
        var to = phases[nextIndex];

        if (directionalLight != null)
        {
            directionalLight.color = Color.Lerp(from.lightColor, to.lightColor, t);
            directionalLight.intensity = Mathf.Lerp(from.intensity, to.intensity, t);
            var rot = Quaternion.Euler(Vector3.Lerp(from.eulerRotation, to.eulerRotation, t));
            directionalLight.transform.rotation = rot;
        }

        // ambient light (best-effort - works for built-in renderer)
        RenderSettings.ambientLight = Color.Lerp(from.lightColor * 0.35f, to.lightColor * 0.35f, t);

        // Skybox blending: if enabled and both phases define skyboxes, blend them
        if (blendSkybox && from.skyboxMaterial != null && to.skyboxMaterial != null)
        {
            // create or reuse the blended instance
            if (blendedSkyboxInstance == null)
            {
                blendedSkyboxInstance = new Material(from.skyboxMaterial);
                blendedSkyboxInstance.hideFlags = HideFlags.DontSave;
            }

            // If shaders differ we cannot safely lerp; just swap when done
            if (from.skyboxMaterial.shader == to.skyboxMaterial.shader)
            {
                blendedSkyboxInstance.Lerp(from.skyboxMaterial, to.skyboxMaterial, t);
                RenderSettings.skybox = blendedSkyboxInstance;
            }
            else
            {
                // fallback: switch at the end of transition
                if (t >= 1f - Mathf.Epsilon)
                    RenderSettings.skybox = to.skyboxMaterial;
                else
                    RenderSettings.skybox = from.skyboxMaterial;
            }
        }
        else
        {
            // if only one phase has a skybox or blending disabled, switch when near target
            if (to.skyboxMaterial != null && t >= 0.999f)
                RenderSettings.skybox = to.skyboxMaterial;
            else if (from.skyboxMaterial != null)
                RenderSettings.skybox = from.skyboxMaterial;
        }

        if (t >= 1f - Mathf.Epsilon)
        {
            // advance
            currentIndex = nextIndex;
            nextIndex = (currentIndex + 1) % phases.Length;
            timer = 0f;

            // if we're at the end and not looping, stop
            if (!loop && currentIndex == phases.Length - 1)
            {
                running = false;
            }
        }
    }

    void ApplyPhaseInstant(DayPhase p)
    {
        if (directionalLight != null)
        {
            directionalLight.color = p.lightColor;
            directionalLight.intensity = p.intensity;
            directionalLight.transform.rotation = Quaternion.Euler(p.eulerRotation);
        }
        RenderSettings.ambientLight = p.lightColor * 0.35f;

        // apply skybox immediately if provided
        if (p.skyboxMaterial != null)
        {
            RenderSettings.skybox = p.skyboxMaterial;
        }
    }

    [ContextMenu("Distribute Durations Equally")]
    void DistributeDurationsEqually()
    {
        if (phases == null || phases.Length == 0) return;
        if (totalCycleSeconds <= 0f)
        {
            Debug.LogWarning("[DayNightCycle] totalCycleSeconds must be > 0 to distribute durations.");
            return;
        }

        float per = totalCycleSeconds / phases.Length;
        for (int i = 0; i < phases.Length; i++)
            phases[i].duration = per;

        Debug.Log($"[DayNightCycle] Distributed {totalCycleSeconds} seconds across {phases.Length} phases ({per}s each)");
    }

    void OnDisable()
    {
        // cleanup generated material to avoid leaking in editor/runtime
        if (blendedSkyboxInstance != null)
        {
            DestroyImmediate(blendedSkyboxInstance);
            blendedSkyboxInstance = null;
        }
    }
}
