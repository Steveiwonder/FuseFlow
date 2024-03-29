namespace FuseFlow.Core;

public interface IJob
{
    bool IsComplete { get; }
    Task Execute(string currentState);
}

