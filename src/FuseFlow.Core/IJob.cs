namespace FuseFlow.Core;

public interface IJob
{
    Task Execute(string currentState);
}

