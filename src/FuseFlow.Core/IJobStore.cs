namespace FuseFlow.Core;

public interface IJobStore
{
    IEnumerable<IJobDetail> GetAllJobs();
    IEnumerable<IJobDetail> GetIncompleteJobs();
    IEnumerable<IJobDetail> GetCompleteJobs();
    void SetJobComplete(IJobDetail jobDetail);
    Task<string> AddJob(Type jobType, string parameters = null);

}


