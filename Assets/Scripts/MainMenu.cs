using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Transition Settings")]
    public Animator transitionAnimator;  // Animator for the fade
    public float transitionTime = 1f;    // duration of fade animation

    [Header("Audio Settings")]
    public AudioSource mainMenuMusic;    // assign your main menu music here

    // Called when "Play" is clicked
    public void Play()
    {
        StartCoroutine(LoadNextScene());
    }

    // Coroutine for smooth fade and transition
    IEnumerator LoadNextScene()
    {
        // Fade out music
        if (mainMenuMusic != null)
        {
            float startVolume = mainMenuMusic.volume;
            while (mainMenuMusic.volume > 0.01f)
            {
                mainMenuMusic.volume -= startVolume * Time.deltaTime / transitionTime;
                yield return null;
            }
            mainMenuMusic.Stop();
        }

        // Start fade animation
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("FadeOut");
        }

        // Wait for animation to finish
        yield return new WaitForSeconds(transitionTime);

        // Load next scene (Cutscene1)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Called when "Quit" is clicked
    public void Quit()
    {
        Application.Quit();
        Debug.Log("Player has quit the game.");
    }
}
