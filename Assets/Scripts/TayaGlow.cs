using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TayaGlow : MonoBehaviour
{
    public Color glowColor = Color.red;
    public float pulseSpeed = 3f;
    public float minIntensity = 1f;
    public float maxIntensity = 2.5f;

    private Material mat;
    private float t;

    void Start()
    {
        var rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // use instance material so we don't modify shared material by accident
            mat = rend.material;
        }
    }

    void Update()
    {
        if (mat == null) return;

        t += Time.deltaTime * pulseSpeed;
        float lerp = (Mathf.Sin(t) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, lerp);
        // Use emission color for the pulsing glow so we don't tint
        // the material's base color (prevents the capsule from
        // turning purple when the glow is applied).
        if (mat.HasProperty("_EmissionColor"))
        {
            Color emission = glowColor * intensity;
            mat.SetColor("_EmissionColor", emission);
            mat.EnableKeyword("_EMISSION");
        }
        else
        {
            // Fallback: if shader has no emission, only set the base color
            mat.color = glowColor * intensity;
        }
    }

    void OnDestroy()
    {
        // cleanup instantiated material to avoid memory leak
        if (mat != null) Destroy(mat);
    }
}
