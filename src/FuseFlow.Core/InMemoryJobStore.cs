namespace FuseFlow.Core;

public class InMemoryJobStore : IJobStore
{
    private IDictionary<string, InMemoryJob> _jobs = new Dictionary<string, InMemoryJob>();
    class InMemoryJob
    {
        public IJobDetail JobDetail { get; set; }
        public bool IsComplete { get; set; }
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

    public void AddJob(JobDetail jobDetail)
    {
        if (jobDetail is null)
        {
            throw new ArgumentNullException(nameof(jobDetail));
        }
        if (_jobs.ContainsKey(jobDetail.JobId))
        {
            throw new Exception($"Job with id {jobDetail.JobId} already exists");
        }

        _jobs.Add(jobDetail.JobId, new InMemoryJob() { JobDetail = jobDetail });
    }
}

