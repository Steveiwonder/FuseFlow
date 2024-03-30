namespace FuseFlow.Core;

public interface IJobDetail
{
    public string JobId { get; }
    public Type JobType { get; }
    public IJob Job { get; set; }
    public string State { get; }
    public string Parameters { get; }
}

