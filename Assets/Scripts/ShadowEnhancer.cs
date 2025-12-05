using UnityEngine;

/// <summary>
/// ShadowEnhancer - Enhances shadow quality and visual appearance.
/// Improves shadow resolution, cascade settings, and light properties for better visuals.
/// Attach to a GameObject in the scene (can be same as GameManager).
/// </summary>
public class ShadowEnhancer : MonoBehaviour
{
    [Header("Shadow Settings")]
    [SerializeField] private ShadowResolution shadowResolution = ShadowResolution.VeryHigh;
    [SerializeField] private float shadowDistance = 200f;
    [SerializeField] private float shadowNormalBias = 0.4f;
    [SerializeField] private float shadowBias = 0.05f;

    [Header("Cascade Settings")]
    [SerializeField] private int shadowCascades = 4;
    [SerializeField] private float cascadeRatio_1 = 0.25f;
    [SerializeField] private float cascadeRatio_2 = 0.5f;
    [SerializeField] private float cascadeRatio_3 = 0.75f;

    [Header("Light Settings")]
    [SerializeField] private float lightIntensity = 1.2f;
    [SerializeField] private float ambientIntensity = 0.5f;

    [Header("Quality Settings")]
    [SerializeField] private bool useHighQualityShaders = true;
    [SerializeField] private AntiAliasing antiAliasing = AntiAliasing.SMAA1x;

    private enum ShadowResolution
    {
        Low = 1024,
        Medium = 2048,
        High = 4096,
        VeryHigh = 8192
    }

    private enum AntiAliasing
    {
        None = 0,
        FXAA = 1,
        SMAA1x = 2,
        SMAA2x = 4,
        SMAA4x = 5
    }

    private void Start()
    {
        ApplyShadowSettings();
    }

    private void ApplyShadowSettings()
    {
        // Set shadow resolution
        QualitySettings.shadowResolution = (UnityEngine.ShadowResolution)(int)shadowResolution;
        QualitySettings.shadowDistance = shadowDistance;

        // Set cascade settings
        QualitySettings.shadowCascades = shadowCascades;

        // Set cascade distribution
        float[] cascadeSplits = new float[shadowCascades - 1];
        if (shadowCascades >= 2) cascadeSplits[0] = cascadeRatio_1;
        if (shadowCascades >= 3) cascadeSplits[1] = cascadeRatio_2;
        if (shadowCascades >= 4) cascadeSplits[2] = cascadeRatio_3;

        QualitySettings.shadowCascade4Split = new Vector3(cascadeRatio_1, cascadeRatio_2, cascadeRatio_3);

        // Apply anti-aliasing
        QualitySettings.antiAliasing = (int)antiAliasing;

        // Find and enhance directional light
        Light directionalLight = FindObjectOfType<Light>();
        if (directionalLight != null && directionalLight.type == LightType.Directional)
        {
            directionalLight.intensity = lightIntensity;
            directionalLight.shadowStrength = 1f;
            directionalLight.shadowNearPlane = 0.2f;
            directionalLight.shadowBias = shadowBias;
            directionalLight.shadowNormalBias = shadowNormalBias;

            // Enable shadow cookie rendering for better shadow definition
            directionalLight.renderMode = LightRenderMode.ForcePixel;
        }

        // Enhance ambient lighting
        RenderSettings.ambientIntensity = ambientIntensity;

        // Apply shader quality settings
        if (useHighQualityShaders)
        {
            QualitySettings.antiAliasing = Mathf.Max((int)antiAliasing, 2);
            QualitySettings.maxQueuedFrames = 3;
        }

        Debug.Log("[ShadowEnhancer] Shadow enhancement applied successfully!");
        Debug.Log($"Shadow Resolution: {shadowResolution} | Shadow Distance: {shadowDistance}");
        Debug.Log($"Cascades: {shadowCascades} | Anti-Aliasing: {antiAliasing}");
    }

    /// <summary>
    /// Reapply shadow settings if changed in inspector during gameplay.
    /// </summary>
    public void RefreshSettings()
    {
        ApplyShadowSettings();
    }
}
