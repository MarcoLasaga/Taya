using UnityEngine;

public class WanderState : BaseState
{
    Vector3 target;

    public override void EnterState(NPCStateMachine npc)
    {
        target = npc.transform.position + new Vector3(
            Random.Range(-10f, 10f),
            0,
            Random.Range(-10f, 10f)
        );

        if (npc.agent != null)
        {
            npc.agent.SetDestination(target);
            npc.agent.isStopped = false;
            npc.agent.speed = npc.wanderSpeed;
        }
        npc.SetRunningAnimation();
    }

    public override void UpdateState(NPCStateMachine npc)
    {
        // FIX: npc.GM instead of npc.manager
        if (npc.GM.currentTaya == npc.gameObject)
        {
            npc.SwitchState(npc.tayaState);
            return;
        }

        if (npc.agent != null && npc.agent.isOnNavMesh)
        {
            if (!npc.agent.pathPending && npc.agent.remainingDistance < 1f)
            {
                npc.SwitchState(npc.wanderState);
            }
        }
    }

    public override void ExitState(NPCStateMachine npc) { }
}
