

namespace FuseFlow.Core;

public interface IState
{
    string Name { get; }
    void Enter(StateMachine stateMachine);
    Task Execute(StateMachine stateMachine);
    void Exit(StateMachine stateMachine);

}

