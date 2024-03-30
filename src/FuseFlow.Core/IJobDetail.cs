namespace FuseFlow.Core;

public interface IJobDetail
{
    string JobId { get; }
    Type JobType { get; }
    IJob Job { get; set; }
    string CurrentState { get; set; }
    string CurrentStateData { get; set; }
    string Parameters { get; }
}

