namespace FuseFlow.Core;

public class JobDispatcher : IJobDispatcher
{
    private readonly IJobStore _jobStore;
    private readonly IDataSerializer _jobParamSerializer;

    public JobDispatcher(IJobStore jobStore, IDataSerializer jobParamSerializer)
    {
        _jobStore = jobStore;
        _jobParamSerializer = jobParamSerializer;
    }
    public async Task<string> Dispatch(Type jobType, object parameters)
    {
        var serializedParams = await _jobParamSerializer.Serialize(parameters);
        return await _jobStore.AddJob(jobType, serializedParams);

    }
    public Task<string> Dispatch(IJob job, object parameters)
    {
        return Dispatch(job.GetType(), parameters);
    }
    public Task<string> Dispatch<TJob>(object parameters = null)
    {
        return Dispatch(typeof(TJob), parameters);
    }
}
