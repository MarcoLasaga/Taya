using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PS2RetroPP : MonoBehaviour
{
    [Header("Render Target")]
    public int renderWidth = 512;
    public int renderHeight = 384;

    [Header("Style")]
    [Range(2, 64)]
    public int colorLevels = 16;
    [Range(0f, 1f)]
    public float scanlineIntensity = 0.25f;
    [Range(0f, 2f)]
    public float saturation = 0.9f;

    public Shader shader;

    private Material mat;

    void Start()
    {
        if (shader == null)
            shader = Shader.Find("Hidden/PS2Palette");

        if (shader == null)
        {
            Debug.LogWarning("PS2RetroPP: Shader 'Hidden/PS2Palette' not found. Disabling component.");
            enabled = false;
            return;
        }

        mat = new Material(shader);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (mat == null)
        {
            Graphics.Blit(src, dest);
            return;
        }

        // create a temporary low-res render target and copy the camera render into it
        RenderTexture small = RenderTexture.GetTemporary(renderWidth, renderHeight, 0);
        small.filterMode = FilterMode.Point;
        Graphics.Blit(src, small);

        mat.SetFloat("_Levels", colorLevels);
        mat.SetFloat("_ScanlineIntensity", scanlineIntensity);
        mat.SetFloat("_Saturation", saturation);

        // blit the low-res texture to the screen using the retro shader (sampling will use point filtering)
        Graphics.Blit(small, dest, mat);

        RenderTexture.ReleaseTemporary(small);
    }

    void OnDisable()
    {
        if (mat)
            DestroyImmediate(mat);
    }
}
