

namespace FuseFlow.Core;

public abstract class State : IState
{
    public abstract string Name { get; }

    public virtual void Enter(StateMachine stateMachine)
    {
    }

    public abstract Task Execute(StateMachine stateMachine);

    public virtual void Exit(StateMachine stateMachine)
    {
    }

}

