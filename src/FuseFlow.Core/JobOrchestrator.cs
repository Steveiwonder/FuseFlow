using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace FuseFlow.Core;

public class JobOrchestrator : IJobOrchestrator
{
    private readonly IJobStore _jobStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobOrchestrator> _logger;
    private IDictionary<string, IJobDetail> _activeJobs = new Dictionary<string, IJobDetail>();

    public JobOrchestrator(IJobStore jobStore, IServiceProvider serviceProvider, ILogger<JobOrchestrator> logger)
    {
        _jobStore = jobStore;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var jobs = _jobStore.GetIncompleteJobs();
        foreach (var job in jobs)
        {
            if (_activeJobs.ContainsKey(job.JobId))
            {
                continue;
            }
            _activeJobs.Add(job.JobId, job);

        }

        foreach (var jobKey in _activeJobs.Keys.ToArray())
        {
            try
            {
                var jobDetail = _activeJobs[jobKey];
                if (jobDetail.Job is null)
                {
                    // Initialise the job
                    jobDetail.Job = (IJob)ActivatorUtilities.CreateInstance(scope.ServiceProvider, jobDetail.JobType);
                    await jobDetail.Job.Configure(jobDetail);
                }
                // remove complete jobs
                if (jobDetail.Job.IsComplete)
                {
                    RemoveActiveJob(jobDetail);
                    continue;
                }
                await jobDetail.Job.Execute(stoppingToken);
                jobDetail.CurrentState = jobDetail.Job.CurrentState;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExecuteAsync");
            }

        }
    }

    private void RemoveActiveJob(IJobDetail jobDetail)
    {
        _jobStore.SetJobComplete(jobDetail);
        _activeJobs.Remove(jobDetail.JobId);
    }

}

