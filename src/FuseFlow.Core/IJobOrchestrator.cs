namespace FuseFlow.Core;

public interface IJobOrchestrator
{
    Task ExecuteAsync(CancellationToken stoppingToken);
}

