namespace FuseFlow.Core;

public interface IJobDispatcher
{
    Task<string> Dispatch(IJob job, object parameters);
    Task<string> Dispatch(Type jobType, object parameters);
}

