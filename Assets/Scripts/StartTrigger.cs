using UnityEngine;

public class StartTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var gm = FindObjectOfType<GameManager>();
        if (gm == null) return;

        // If game hasn't been unlocked yet, treat this as the start-cube trigger
        if (!gm.gameUnlocked)
        {
            gm.OnStartCubeTriggered(other.gameObject);
            // keep the trigger active so it can be used again at end
            return;
        }

        // If the game has ended and is awaiting the end interaction,
        // do nothing here — `EndScene` trigger will handle scene progression.
        // This keeps end handling centralized in `EndScene`.
    }
}
