using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FuseFlow.Core;

public abstract class StateMachineJob<TParam> : StateMachineJob where TParam : class
{
    private TParam _params;
    protected StateMachineJob(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public async Task<TParam> GetParams()
    {
        if (string.IsNullOrEmpty(JobDetail.Parameters))
        {
            return default;
        }
        if (_params != default)
        {
            return _params;
        }
        return _params = await DataSerializer.Deserialize<TParam>(JobDetail.Parameters);
    }
}
public abstract class StateMachineJob : IJob
{
    protected ILogger Logger;
    protected StateMachine StateMachine { get; private set; }
    protected IDataSerializer DataSerializer { get; private set; }
    public bool IsComplete { get; private set; }
    public string CurrentState { get; private set; }
    protected IJobDetail JobDetail { get; private set; }

    private readonly IJobStateStore _jobStateStore;

    public StateMachineJob(IServiceProvider serviceProvider)
    {
        _jobStateStore = serviceProvider.GetRequiredService<IJobStateStore>();
        Logger = serviceProvider.GetRequiredService<ILogger>();
        DataSerializer = serviceProvider.GetRequiredService<IDataSerializer>();
        StateMachine = new StateMachine(Logger, serviceProvider, () =>
        {
            return _jobStateStore.GetStateData(JobDetail.JobId);
        }, (data) =>
        {
            _jobStateStore.SetStateData(JobDetail.JobId, data);
        },
        () => JobDetail.Job as StateMachineJob);
    }


    public async Task Execute(CancellationToken stoppingToken)
    {
        if (IsComplete)
        {
            Logger.LogInformation("This job has complete");
            return;
        }



        Logger.LogInformation("Executing state machine");
        await StateMachine.Execute();
        Logger.LogInformation("Execution complete");
        if (StateMachine.CurrentState == null)
        {
            Logger.LogInformation("State machine has no state, job is complete");
            IsComplete = true;
            CurrentState = null;
            return;
        }


        var currentState = StateMachine.CurrentState.Name;
        if (currentState != CurrentState)
        {
            Logger.LogInformation($"State machine has transitioned from {CurrentState} -> {currentState}");
            // the state has transitioned within the call to Execute();
            // store the new state so it can continue if something fails
            CurrentState = StateMachine.CurrentState.Name;
        }

    }

    protected abstract Type GetStateType(string state);

    public virtual Task Configure(IJobDetail jobDetail)
    {
        JobDetail = jobDetail;
        Logger.LogInformation("Getting initial state");
        var initialStateType = GetStateType(jobDetail.CurrentState);
        if (initialStateType == null)
        {
            throw new Exception("Cannot find initial state");
        }
        // set the initial state
        Logger.LogInformation("Setting state machine initial state");
        StateMachine.ChangeState(initialStateType);
        CurrentState = StateMachine.CurrentState.Name;
        Logger.LogInformation($"Initial state set {CurrentState}");

        return Task.CompletedTask;
    }

    public T GetJob<T>() where T : StateMachineJob
    {
        return JobDetail?.Job as T;
    }
}

