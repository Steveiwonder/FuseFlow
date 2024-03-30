namespace FuseFlow.Core;

public interface IJob
{
    bool IsComplete { get; }
    Task Execute(CancellationToken stoppingToken, string currentState);
    Task Configure(IJobDetail jobDetail);
}

