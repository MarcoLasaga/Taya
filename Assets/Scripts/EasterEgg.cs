using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Attach this to the GameManager GameObject to add a 20-minute easter egg timer.
/// If the player waits 20 minutes without starting the game, a secret scene will load.
/// </summary>
public class EasterEgg : MonoBehaviour
{
    [Header("Easter Egg Settings")]
    [Tooltip("Scene to load if player waits 20 minutes without starting the game")]
    public string secretScene = "";

    [Tooltip("Time in seconds before easter egg triggers (default 1200 = 20 minutes)")]
    public float waitTime = 1200f;

    [Tooltip("Subtitle text to show when easter egg triggers")]
    public string triggerSubtitle = "You found the secret ending...";

    private float timer = 0f;
    private bool triggered = false;
    private GameManager gm;
    private TextMeshProUGUI subtitlesText;

    void Start()
    {
        gm = GetComponent<GameManager>();
        if (gm != null)
            subtitlesText = gm.subtitlesText;
    }

    void Update()
    {
        // Only count time if game hasn't started and easter egg hasn't triggered yet
        if (gm != null && !gm.gameRunning && !gm.gameUnlocked && !triggered)
        {
            timer += Time.deltaTime;
            if (timer >= waitTime)
            {
                triggered = true;
                TriggerEasterEgg();
            }
        }
    }

    void TriggerEasterEgg()
    {
        Debug.Log($"[EasterEgg] Triggered after {waitTime} seconds!");

        // Show subtitle if available
        if (subtitlesText != null && !string.IsNullOrEmpty(triggerSubtitle))
        {
            subtitlesText.text = triggerSubtitle;
            subtitlesText.gameObject.SetActive(true);
        }

        // Load the secret scene if assigned
        if (!string.IsNullOrEmpty(secretScene))
        {
            StartCoroutine(WaitAndLoadScene(2f));
        }
        else
        {
            Debug.LogWarning("[EasterEgg] Secret scene not assigned!");
        }
    }

    System.Collections.IEnumerator WaitAndLoadScene(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(secretScene);
    }
}
