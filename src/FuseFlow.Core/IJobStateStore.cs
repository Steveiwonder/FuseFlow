namespace FuseFlow.Core;

public interface IJobStateStore
{
    void SetStateData(string jobId, string data);
    string GetStateData(string jobId);
}


