namespace FuseFlow.Core;

public class InMemoryJobStore : IJobStore, IJobStateStore
{
    private IDictionary<string, InMemoryJob> _jobs = new Dictionary<string, InMemoryJob>();
    class InMemoryJob
    {
        public IJobDetail JobDetail { get; set; }
        public bool IsComplete { get; set; }
    }

    public InMemoryJobStore()
    {

    }

    public Task<string> AddJob(Type jobType, string parameters = null)
    {
        if (!jobType.IsAssignableTo(typeof(IJob)))
        {
            throw new Exception($"Invalid type, must implement {nameof(IJob)}");
        }
        var jobId = Guid.NewGuid().ToString();
        var jobDetail = new JobDetail()
        {
            JobType = jobType,
            JobId = jobId,
            Parameters = parameters
        };
        _jobs.Add(jobId, new InMemoryJob() { JobDetail = jobDetail });

        return Task.FromResult(jobId);
    }
    public IEnumerable<IJobDetail> GetAllJobs()
    {
        return _jobs.Values.Select(d => d.JobDetail);
    }

    public IEnumerable<IJobDetail> GetCompleteJobs()
    {
        return _jobs.Values.Where(d => d.IsComplete).Select(d => d.JobDetail);
    }

    public IEnumerable<IJobDetail> GetIncompleteJobs()
    {
        return _jobs.Values.Where(d => !d.IsComplete).Select(d => d.JobDetail);
    }

    public void SetJobComplete(IJobDetail jobDetail)
    {
        if (_jobs.TryGetValue(jobDetail.JobId, out InMemoryJob inMemoryJob))
        {
            inMemoryJob.IsComplete = true;
        }
    }

    public void SetStateData(string jobId, string data)
    {
        if (!_jobs.ContainsKey(jobId))
        {
            throw new Exception($"Job id {jobId} does not exist");
        }
        _jobs[jobId].JobDetail.CurrentStateData = data;
    }

    public string GetStateData(string jobId)
    {

        if (!_jobs.ContainsKey(jobId))
        {
            throw new Exception($"Job id {jobId} does not exist");
        }

        return _jobs[jobId].JobDetail.CurrentStateData;
    }
}

