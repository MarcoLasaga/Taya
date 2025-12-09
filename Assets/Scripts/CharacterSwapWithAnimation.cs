using UnityEngine;

/// <summary>
/// Simple two-character swapper. Press a key (default Tab) to toggle which
/// character is active. It preserves position/rotation and keeps camera follow
/// pointed at the current character if you assign a follow component.
/// </summary>
public class CharacterSwapWithAnimation : MonoBehaviour
{
    [Header("Characters")]
    public GameObject character1;
    public GameObject character2;

    [Header("Input")]
    public KeyCode swapKey = KeyCode.Tab;

    private GameObject currentCharacter;

    void Start()
    {
        // Default to character1 active, character2 inactive
        currentCharacter = character1;
        if (character1 != null) character1.SetActive(true);
        if (character2 != null) character2.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(swapKey))
        {
            SwitchCharacter();
        }
    }

    void SwitchCharacter()
    {
        if (character1 == null || character2 == null)
            return;

        if (currentCharacter == character1)
        {
            // Transfer pose
            character2.transform.SetPositionAndRotation(character1.transform.position, character1.transform.rotation);

            // Toggle
            character1.SetActive(false);
            character2.SetActive(true);
            currentCharacter = character2;
        }
        else
        {
            character1.transform.SetPositionAndRotation(character2.transform.position, character2.transform.rotation);

            character2.SetActive(false);
            character1.SetActive(true);
            currentCharacter = character1;
        }
    }
}

