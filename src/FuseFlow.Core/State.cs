

namespace FuseFlow.Core;

public abstract class State<TJob> : IState where TJob : StateMachineJob
{
    public abstract string Name { get; }
    protected TJob Job => _job;
    private TJob _job;
    public void SetJob(TJob job)
    {
        _job = job;
    }

    public virtual void Enter(StateMachine stateMachine)
    {
    }

    public abstract Task Execute(StateMachine stateMachine);

    public virtual void Exit(StateMachine stateMachine)
    {
    }
}

