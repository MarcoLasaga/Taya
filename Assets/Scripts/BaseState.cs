using UnityEngine;

public abstract class BaseState
{
    public abstract void EnterState(NPCStateMachine npc);
    public abstract void UpdateState(NPCStateMachine npc);
    public abstract void ExitState(NPCStateMachine npc);
}
