using UnityEngine;

public class EscapeState : BaseState
{
    public override void EnterState(NPCStateMachine npc)
    {
        npc.agent.speed = npc.escapeSpeed;
    }

    public override void UpdateState(NPCStateMachine npc)
    {
        // FIX: npc.GM instead of npc.manager
        GameObject taya = npc.GM.currentTaya;
        if (taya == null) return;

        Vector3 dir = (npc.transform.position - taya.transform.position).normalized;
        Vector3 newPos = npc.transform.position + dir * 5f;

        npc.agent.SetDestination(newPos);

        if (Vector3.Distance(npc.transform.position, taya.transform.position) > 12f)
        {
            npc.SwitchState(npc.wanderState);
        }
    }

    public override void ExitState(NPCStateMachine npc)
    {
        npc.agent.speed = npc.wanderSpeed;
    }
}
