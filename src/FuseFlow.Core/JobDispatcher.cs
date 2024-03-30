using System.Text.Json;

namespace FuseFlow.Core;

public class JobDispatcher : IJobDispatcher
{
    private readonly IJobStore _jobStore;

    public JobDispatcher(IJobStore jobStore)
    {
        _jobStore = jobStore;
    }
    public Task<string> Dispatch(Type jobType, Dictionary<string, object> parameters)
    {
        var serializedParams = JsonSerializer.Serialize(parameters);
        return _jobStore.AddJob(jobType, serializedParams);

    }
    public Task<string> Dispatch(IJob job, Dictionary<string, object> parameters)
    {
        return Dispatch(job.GetType(), parameters);
    }
}

