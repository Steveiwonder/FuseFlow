namespace FuseFlow.Core;

public class JobDetail : IJobDetail
{
    public string JobId { get; set; }
    public Type JobType { get; set; }
    public IJob Job { get; set; }
    public string CurrentState { get; set; }
    public string CurrentStateData { get; set; }
    public string Parameters { get; set; }
}

