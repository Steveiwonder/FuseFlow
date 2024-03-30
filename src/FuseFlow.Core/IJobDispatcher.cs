namespace FuseFlow.Core;

public interface IJobDispatcher
{
    Task<string> Dispatch<TJob>(object parameters = null);
    Task<string> Dispatch(IJob job, object parameters = null);
    Task<string> Dispatch(Type jobType, object parameters = null);
}

