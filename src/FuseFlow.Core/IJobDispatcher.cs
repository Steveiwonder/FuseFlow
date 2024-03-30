namespace FuseFlow.Core;

public interface IJobDispatcher
{
    Task<string> Dispatch(IJob job, Dictionary<string, object> parameters);
    Task<string> Dispatch(Type jobType, Dictionary<string, object> parameters);
}

