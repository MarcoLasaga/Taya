using UnityEngine;

public class ChaseState : BaseState
{
    public override void EnterState(NPCStateMachine npc)
    {
        npc.agent.speed = npc.tayaSpeed;
    }

    public override void UpdateState(NPCStateMachine npc)
    {
        if (!npc.isTaya)
        {
            npc.SwitchState(npc.wanderState);
            return;
        }

        GameObject target = npc.GM.GetNearestNonTaya(npc.transform.position);
        if (target == null) return;

        npc.agent.SetDestination(target.transform.position);
    }

    public override void ExitState(NPCStateMachine npc) { }
}
