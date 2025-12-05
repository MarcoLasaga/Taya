using UnityEngine;

public class TayaState : BaseState
{
    public override void EnterState(NPCStateMachine npc)
    {
        npc.agent.speed = npc.tayaSpeed;
        // mark this NPC as the active Taya and reset chase helpers
        npc.isTaya = true;
        npc.chaseTarget = null;
        npc.chaseTimer = 0f;
    }

    public override void UpdateState(NPCStateMachine npc)
    {
        // choose or validate current chase target
        if (npc.chaseTarget == null)
        {
            npc.chaseTarget = npc.GM.GetNearestNonTaya(npc.transform.position);
            npc.chaseTimer = 0f;
        }

        if (npc.chaseTarget != null)
        {
            // set destination
            npc.agent.SetDestination(npc.chaseTarget.transform.position);

            // increment timer; if exceeded, pick a different target
            npc.chaseTimer += Time.deltaTime;
            if (npc.chaseTimer >= npc.tagTimeout)
            {
                // try to find another target excluding the current one
                GameObject next = npc.GM.GetNearestNonTayaExcluding(npc.transform.position, npc.chaseTarget);
                npc.chaseTarget = next;
                npc.chaseTimer = 0f;
            }
        }
    }

    public override void ExitState(NPCStateMachine npc)
    {
        npc.agent.speed = npc.wanderSpeed;
        // clear taya flag and chase helpers
        npc.isTaya = false;
        npc.chaseTarget = null;
        npc.chaseTimer = 0f;
    }
}
