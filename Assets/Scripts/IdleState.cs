using UnityEngine;

public class IdleState : BaseState
{
    private float idleDuration = 2f;
    private float elapsed = 0f;

    public override void EnterState(NPCStateMachine npc)
    {
        idleDuration = Random.Range(1.0f, 3.0f);
        elapsed = 0f;
        if (npc.agent != null) npc.agent.isStopped = true;
        npc.SetIdleAnimation();
    }

    public override void UpdateState(NPCStateMachine npc)
    {
        elapsed += Time.deltaTime;
        if (elapsed >= idleDuration)
        {
            npc.SwitchState(npc.wanderState);
        }
    }

    public override void ExitState(NPCStateMachine npc)
    {
        if (npc.agent != null) npc.agent.isStopped = false;
    }
}
