using UnityEngine;

public class ChaseState : BaseState
{
    public override void EnterState(NPCStateMachine npc)
    {
        if (npc.agent != null)
        {
            npc.agent.isStopped = false;
            npc.agent.speed = npc.tayaSpeed;
        }
        npc.SetRunningAnimation();
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

        if (npc.agent != null)
            npc.agent.SetDestination(target.transform.position);
    }

    public override void ExitState(NPCStateMachine npc) { }
}
