using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuMusic : MonoBehaviour
{
    private AudioSource audioSource;
    public float fadeDuration = 2f; // seconds to fade out

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If the new scene is not the main menu, start fading out
        if (scene.name != "MainMenu") // replace with your actual main menu scene name
        {
            StartCoroutine(FadeOutAndStop());
        }
    }

    IEnumerator FadeOutAndStop()
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.Stop();
        Destroy(gameObject); // destroy it so it doesn’t continue in the next scene
    }
}
