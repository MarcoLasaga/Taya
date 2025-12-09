using UnityEngine;

public class StartTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var gm = FindObjectOfType<GameManager>();
        if (gm == null) return;

        // If we're awaiting the end interaction (after the game ended), ignore
        // start-cube triggers so the player can't accidentally restart the game
        if (gm.awaitingEndInteraction) return;

        // If game hasn't been unlocked yet, treat this as the start-cube trigger
        if (!gm.gameUnlocked)
        {
            gm.OnStartCubeTriggered(other.gameObject);
            return;
        }

        // If the game has ended and is awaiting the end interaction,
        // do nothing here — `EndScene` trigger will handle scene progression.
        // This keeps end handling centralized in `EndScene`.
    }
}
