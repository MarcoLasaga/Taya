using UnityEngine;

public class IdleState : BaseState
{
    public override void EnterState(NPCStateMachine npc)
    {
        npc.agent.isStopped = true;
    }

    public override void UpdateState(NPCStateMachine npc)
    {
        // FIX: use npc.wanderState
        npc.SwitchState(npc.wanderState);
    }

    public override void ExitState(NPCStateMachine npc)
    {
        npc.agent.isStopped = false;
    }
}
