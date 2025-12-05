using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TayaProximityAlert - Makes the screen red when Taya (current friend) is near the player.
/// Creates a red vignette overlay on the screen that intensifies as Taya gets closer.
/// Attach to the main Camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class TayaProximityAlert : MonoBehaviour
{
    [Header("Proximity Settings")]
    [SerializeField]
    private float alertDistance = 15f;

    [SerializeField]
    private float maxAlertDistance = 30f;

    [Header("Visual Effect")]
    [SerializeField]
    private float maxRedIntensity = 0.4f;

    [SerializeField]
    private float fadeSpeed = 3f;

    [SerializeField]
    private Color alertColor = new Color(1f, 0f, 0f, 1f);

    [Header("References")]
    [SerializeField]
    private GameManager gameManager;

    private Camera mainCamera;
    private float currentRedIntensity = 0f;
    private Image screenOverlay;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        // Create canvas and image for red overlay
        CreateRedOverlay();
    }

    private void CreateRedOverlay()
    {
        // Check if overlay already exists
        Transform existingOverlay = transform.Find("ProximityAlertOverlay");
        if (existingOverlay != null)
        {
            screenOverlay = existingOverlay.GetComponent<Image>();
            return;
        }

        // Create a canvas for the overlay
        GameObject canvasObj = new GameObject("ProximityAlertOverlay");
        canvasObj.transform.SetParent(mainCamera.transform, false);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Create the red image
        GameObject imageObj = new GameObject("RedVignette");
        imageObj.transform.SetParent(canvasObj.transform, false);

        screenOverlay = imageObj.AddComponent<Image>();
        // Initialize overlay using configured alertColor but fully transparent so it fades in properly
        screenOverlay.color = new Color(alertColor.r, alertColor.g, alertColor.b, 0f);

        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private void Update()
    {
        // If GameManager is missing, no current Taya, the game isn't running,
        // or the player is the current Taya, fade out and skip overlay.
        if (gameManager == null || gameManager.currentTaya == null || !gameManager.gameRunning || gameManager.currentTaya == gameManager.player)
        {
            // Fade out red if not in game, no Taya, or player is the Taya
            currentRedIntensity = Mathf.Lerp(currentRedIntensity, 0f, Time.deltaTime * fadeSpeed);
            UpdateOverlayAlpha();
            return;
        }

        // Calculate distance to current Taya
        float distanceToTaya = Vector3.Distance(gameManager.player.transform.position, gameManager.currentTaya.transform.position);

        // Calculate red intensity based on distance
        if (distanceToTaya < alertDistance)
        {
            // Within alert range - full red
            currentRedIntensity = Mathf.Lerp(currentRedIntensity, maxRedIntensity, Time.deltaTime * fadeSpeed);
        }
        else if (distanceToTaya < maxAlertDistance)
        {
            // Between alert and max distance - gradient red
            float distanceRatio = 1f - ((distanceToTaya - alertDistance) / (maxAlertDistance - alertDistance));
            float targetIntensity = maxRedIntensity * distanceRatio;
            currentRedIntensity = Mathf.Lerp(currentRedIntensity, targetIntensity, Time.deltaTime * fadeSpeed);
        }
        else
        {
            // Outside max distance - fade out
            currentRedIntensity = Mathf.Lerp(currentRedIntensity, 0f, Time.deltaTime * fadeSpeed);
        }

        UpdateOverlayAlpha();
    }

    private void UpdateOverlayAlpha()
    {
        if (screenOverlay != null)
        {
            Color newColor = screenOverlay.color;
            newColor.a = currentRedIntensity;
            screenOverlay.color = newColor;
        }
    }

    /// <summary>
    /// Set the alert color (default red).
    /// </summary>
    public void SetAlertColor(Color color)
    {
        alertColor = color;
        if (screenOverlay != null)
        {
            screenOverlay.color = new Color(color.r, color.g, color.b, screenOverlay.color.a);
        }
    }

    /// <summary>
    /// Set alert distance (how close before screen turns red).
    /// </summary>
    public void SetAlertDistance(float distance)
    {
        alertDistance = distance;
    }

    /// <summary>
    /// Set max alert distance (fade stops after this distance).
    /// </summary>
    public void SetMaxAlertDistance(float distance)
    {
        maxAlertDistance = distance;
    }

    /// <summary>
    /// Get current red intensity (0-1).
    /// </summary>
    public float GetCurrentIntensity()
    {
        return currentRedIntensity;
    }
}
