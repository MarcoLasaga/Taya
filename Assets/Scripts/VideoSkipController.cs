using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class VideoSkipController : MonoBehaviour
{
    [Header("UI")]
    public Button skipButton;
    public CanvasGroup skipCanvasGroup;

    [Header("Timing")]
    public float delay = 5f;
    public float fadeDuration = 0.6f;

    private void Awake()
    {
        if (skipButton != null && skipCanvasGroup == null)
            skipCanvasGroup = skipButton.GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (skipButton == null)
        {
            Debug.LogError("[VideoSkipController] skipButton is not assigned in the Inspector.");
            return;
        }

        if (skipCanvasGroup == null)
        {

            skipCanvasGroup = skipButton.gameObject.AddComponent<CanvasGroup>();
        }


        skipCanvasGroup.alpha = 0f;
        skipCanvasGroup.interactable = false;
        skipCanvasGroup.blocksRaycasts = false;
        skipButton.gameObject.SetActive(true);


        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(SkipCutscene);


        StartCoroutine(ShowSkipAfterDelay());
    }

    private IEnumerator ShowSkipAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(FadeCanvasGroup(skipCanvasGroup, skipCanvasGroup.alpha, 1f, fadeDuration));
        skipCanvasGroup.interactable = true;
        skipCanvasGroup.blocksRaycasts = true;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }


    public void SkipCutscene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
