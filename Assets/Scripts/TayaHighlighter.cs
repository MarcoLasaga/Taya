using UnityEngine;

/// <summary>
/// TayaHighlighter - Desaturates all characters except current Taya, adds red glow/trail to Taya.
/// Attach to GameManager or any active GameObject in the scene.
/// </summary>
public class TayaHighlighter : MonoBehaviour
{
    [Header("Taya Highlight")]
    [SerializeField]
    private Color tayaGlowColor = new Color(1f, 0f, 0f, 1f);

    [SerializeField]
    private float tayaGlowIntensity = 2f;

    [SerializeField]
    private bool showTayaTrail = true;

    [Header("Other Characters")]
    [SerializeField]
    private float desaturationAmount = 0.7f;

    [SerializeField]
    private float fadeSpeed = 2f;

    [Header("References")]
    [SerializeField]
    private GameManager gameManager;

    private GameObject lastTaya;
    private Material tayaGlowMaterial;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        // Create glow material for Taya
        tayaGlowMaterial = new Material(Shader.Find("Standard"));
        tayaGlowMaterial.SetColor("_EmissionColor", tayaGlowColor * tayaGlowIntensity);
    }

    private void Update()
    {
        if (gameManager == null || !gameManager.gameRunning)
        {
            return;
        }

        // Check if Taya changed
        if (gameManager.currentTaya != lastTaya)
        {
            // Remove highlight from previous Taya
            if (lastTaya != null)
            {
                RemoveTayaHighlight(lastTaya);
            }

            // Add highlight to current Taya
            if (gameManager.currentTaya != null)
            {
                HighlightCurrentTaya(gameManager.currentTaya);
                lastTaya = gameManager.currentTaya;
            }
        }

        // Desaturate other characters
        DesaturateOtherCharacters();
    }

    private void HighlightCurrentTaya(GameObject taya)
    {
        if (taya == null) return;

        // Add red glow/emission to Taya
        SkinnedMeshRenderer[] skinnedMeshes = taya.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var mesh in skinnedMeshes)
        {
            foreach (var mat in mesh.materials)
            {
                mat.SetFloat("_Smoothness", 0f);
                mat.SetColor("_EmissionColor", tayaGlowColor * tayaGlowIntensity);
            }
        }

        MeshRenderer[] meshes = taya.GetComponentsInChildren<MeshRenderer>();
        foreach (var mesh in meshes)
        {
            foreach (var mat in mesh.materials)
            {
                mat.SetFloat("_Smoothness", 0f);
                mat.SetColor("_EmissionColor", tayaGlowColor * tayaGlowIntensity);
            }
        }

        // Add trail renderer for red trail effect
        if (showTayaTrail)
        {
            AddTrailToTaya(taya);
        }

        Debug.Log($"[TayaHighlighter] Taya '{taya.name}' highlighted with red glow!");
    }

    private void RemoveTayaHighlight(GameObject taya)
    {
        if (taya == null) return;

        // Remove emission glow
        SkinnedMeshRenderer[] skinnedMeshes = taya.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var mesh in skinnedMeshes)
        {
            foreach (var mat in mesh.materials)
            {
                mat.SetColor("_EmissionColor", Color.black);
            }
        }

        MeshRenderer[] meshes = taya.GetComponentsInChildren<MeshRenderer>();
        foreach (var mesh in meshes)
        {
            foreach (var mat in mesh.materials)
            {
                mat.SetColor("_EmissionColor", Color.black);
            }
        }

        // Remove trail renderer
        TrailRenderer trail = taya.GetComponent<TrailRenderer>();
        if (trail != null)
        {
            Destroy(trail);
        }
    }

    private void DesaturateOtherCharacters()
    {
        if (gameManager == null || gameManager.allFriends == null)
        {
            return;
        }

        foreach (var friend in gameManager.allFriends)
        {
            if (friend == null || friend == gameManager.currentTaya)
            {
                continue;
            }

            // Desaturate non-current Taya characters
            SkinnedMeshRenderer[] skinnedMeshes = friend.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var mesh in skinnedMeshes)
            {
                foreach (var mat in mesh.materials)
                {
                    ApplyDesaturation(mat, desaturationAmount);
                }
            }

            MeshRenderer[] meshes = friend.GetComponentsInChildren<MeshRenderer>();
            foreach (var mesh in meshes)
            {
                foreach (var mat in mesh.materials)
                {
                    ApplyDesaturation(mat, desaturationAmount);
                }
            }
        }
    }

    private void ApplyDesaturation(Material mat, float amount)
    {
        if (mat == null) return;

        // Reduce saturation by lowering emission and adjusting color.
        // We smoothly interpolate towards the desaturated color using
        // `fadeSpeed` so the change is visually pleasing instead of instant.
        if (!mat.HasProperty("_Color")) return;

        Color currentColor = mat.GetColor("_Color");

        // Convert to grayscale
        float gray = (currentColor.r + currentColor.g + currentColor.b) / 3f;
        Color target = Color.Lerp(currentColor, new Color(gray, gray, gray, currentColor.a), amount);

        // Smoothly approach the target color using fadeSpeed
        float t = Mathf.Clamp01(fadeSpeed * Time.deltaTime);
        Color next = Color.Lerp(currentColor, target, t);
        mat.SetColor("_Color", next);
    }

    private void AddTrailToTaya(GameObject taya)
    {
        // Remove existing trail if any
        TrailRenderer existingTrail = taya.GetComponent<TrailRenderer>();
        if (existingTrail != null)
        {
            Destroy(existingTrail);
        }

        // Add trail renderer
        TrailRenderer trail = taya.AddComponent<TrailRenderer>();
        trail.time = 0.5f; // Trail duration
        trail.startWidth = 0.5f;
        trail.endWidth = 0.1f;
        trail.startColor = new Color(1f, 0f, 0f, 0.8f); // Red with transparency
        trail.endColor = new Color(1f, 0f, 0f, 0f); // Fade to transparent
        trail.material = new Material(Shader.Find("Sprites/Default"));
    }

    /// <summary>
    /// Set the color of Taya's glow.
    /// </summary>
    public void SetTayaGlowColor(Color color)
    {
        tayaGlowColor = color;
    }

    /// <summary>
    /// Set the intensity of Taya's glow.
    /// </summary>
    public void SetTayaGlowIntensity(float intensity)
    {
        tayaGlowIntensity = intensity;
    }

    /// <summary>
    /// Set desaturation amount for other characters (0-1).
    /// </summary>
    public void SetDesaturationAmount(float amount)
    {
        desaturationAmount = Mathf.Clamp01(amount);
    }
}
