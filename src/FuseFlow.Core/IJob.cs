namespace FuseFlow.Core;

public interface IJob
{
    string CurrentState { get; }
    bool IsComplete { get; }
    Task Execute(CancellationToken stoppingToken);
    Task Configure(IJobDetail jobDetail);
}

