using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to the end-cube trigger. When the player enters, this will load
/// the next scene (or `GameManager.nextSceneName` if set).
/// </summary>
public class EndScene : MonoBehaviour
{
    [Tooltip("Disable this trigger after it fires once")]
    public bool disableAfterTrigger = true;

    [Tooltip("If true, only trigger when GameManager.awaitingEndInteraction is true")]
    public bool requireAwaitingEndInteraction = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var gm = FindObjectOfType<GameManager>();
        if (gm == null) return;

        if (requireAwaitingEndInteraction && !gm.awaitingEndInteraction) return;

        // If GameManager has a nextSceneName, use it. Otherwise fall back to build index +1
        if (!string.IsNullOrEmpty(gm.nextSceneName))
        {
            SceneManager.LoadScene(gm.nextSceneName);
        }
        else
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(next);
            else
                Debug.Log("[EndScene] No next scene in build settings.");
        }

        if (disableAfterTrigger)
            gameObject.SetActive(false);
    }
}
